param(
    [string]$RootNamespace = "HMS"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = (Get-Location).Path

$SolutionFile = Get-ChildItem -Path $ProjectRoot -File |
    Where-Object {
        $_.Extension -eq ".sln" -or
        $_.Extension -eq ".slnx"
    } |
    Select-Object -First 1

if (-not $SolutionFile) {
    Write-Host "No solution file found. Creating HMS.sln..."
    dotnet new sln --name "HMS"

    $SolutionFile = Get-ChildItem `
        -Path $ProjectRoot `
        -File `
        -Include "*.sln", "*.slnx" |
        Select-Object -First 1
}

$SolutionPath = $SolutionFile.FullName

Write-Host "Using solution: $SolutionPath" -ForegroundColor Cyan

function New-DirectoryIfMissing {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        New-Item -Path $Path -ItemType Directory -Force | Out-Null
        Write-Host "Created directory: $Path"
    }
}

function New-ClassLibraryProject {
    param(
        [Parameter(Mandatory)]
        [string]$ProjectName,

        [Parameter(Mandatory)]
        [string]$ProjectDirectory
    )

    $ProjectFile = Join-Path $ProjectDirectory "$ProjectName.csproj"

    if (-not (Test-Path $ProjectFile)) {
        New-DirectoryIfMissing -Path $ProjectDirectory

        dotnet new classlib `
            --name $ProjectName `
            --output $ProjectDirectory `
            --framework net10.0 `
            --no-restore |
            Out-Host

        $DefaultClass = Join-Path $ProjectDirectory "Class1.cs"

        if (Test-Path $DefaultClass) {
            Remove-Item $DefaultClass -Force
        }

        Write-Host "Created project: $ProjectName" -ForegroundColor Green
    }
    else {
        Write-Host "Project already exists: $ProjectName" -ForegroundColor Yellow
    }

    $ExistingProjects = dotnet sln $SolutionPath list |
        Out-String

    $ProjectFileName = Split-Path $ProjectFile -Leaf

    if ($ExistingProjects -notmatch [regex]::Escape($ProjectFileName)) {
        dotnet sln $SolutionPath add $ProjectFile |
            Out-Host
    }

    return [string]$ProjectFile
}

function Add-ProjectReference {
    param(
        [Parameter(Mandatory)]
        [string]$FromProject,

        [Parameter(Mandatory)]
        [string]$ToProject
    )

    if (-not (Test-Path $FromProject)) {
        throw "Source project not found: $FromProject"
    }

    if (-not (Test-Path $ToProject)) {
        throw "Referenced project not found: $ToProject"
    }

    $ExistingReferences = dotnet list $FromProject reference |
        Out-String

    $ReferencedFileName = Split-Path $ToProject -Leaf

    if ($ExistingReferences -notmatch [regex]::Escape($ReferencedFileName)) {
        dotnet add $FromProject reference $ToProject |
            Out-Host
    }
    else {
        Write-Host "Reference already exists: $ReferencedFileName" `
            -ForegroundColor Yellow
    }
}
function New-CodeDirectories {
    param(
        [Parameter(Mandatory)]
        [string]$BasePath,

        [Parameter(Mandatory)]
        [string[]]$Directories
    )

    foreach ($Directory in $Directories) {
        $FullPath = Join-Path $BasePath $Directory
        New-DirectoryIfMissing -Path $FullPath
    }
}

# ------------------------------------------------------------
# Root directories
# ------------------------------------------------------------

$SourceRoot = Join-Path $ProjectRoot "src"
$BuildingBlocksRoot = Join-Path $SourceRoot "BuildingBlocks"
$ModulesRoot = Join-Path $SourceRoot "Modules"
$TestsRoot = Join-Path $ProjectRoot "tests"
$DeploymentRoot = Join-Path $ProjectRoot "deployment"

New-DirectoryIfMissing -Path $SourceRoot
New-DirectoryIfMissing -Path $BuildingBlocksRoot
New-DirectoryIfMissing -Path $ModulesRoot
New-DirectoryIfMissing -Path $TestsRoot
New-DirectoryIfMissing -Path $DeploymentRoot

# ------------------------------------------------------------
# Building-block projects
# ------------------------------------------------------------

$SharedKernelDirectory =
    Join-Path $BuildingBlocksRoot "$RootNamespace.SharedKernel"

$ApplicationAbstractionsDirectory =
    Join-Path $BuildingBlocksRoot "$RootNamespace.Application.Abstractions"

$SharedInfrastructureDirectory =
    Join-Path $BuildingBlocksRoot "$RootNamespace.Infrastructure"

$ContractsDirectory =
    Join-Path $BuildingBlocksRoot "$RootNamespace.Contracts"

$SharedKernelProject = New-ClassLibraryProject `
    -ProjectName "$RootNamespace.SharedKernel" `
    -ProjectDirectory $SharedKernelDirectory

$ApplicationAbstractionsProject = New-ClassLibraryProject `
    -ProjectName "$RootNamespace.Application.Abstractions" `
    -ProjectDirectory $ApplicationAbstractionsDirectory

$SharedInfrastructureProject = New-ClassLibraryProject `
    -ProjectName "$RootNamespace.Infrastructure" `
    -ProjectDirectory $SharedInfrastructureDirectory

$ContractsProject = New-ClassLibraryProject `
    -ProjectName "$RootNamespace.Contracts" `
    -ProjectDirectory $ContractsDirectory

New-CodeDirectories -BasePath $SharedKernelDirectory -Directories @(
    "Domain",
    "Results",
    "Auditing"
)

New-CodeDirectories -BasePath $ApplicationAbstractionsDirectory -Directories @(
    "CQRS",
    "Behaviors",
    "Authentication",
    "Tenancy",
    "Clock",
    "Messaging"
)

New-CodeDirectories -BasePath $SharedInfrastructureDirectory -Directories @(
    "Authentication",
    "Authorization",
    "Caching",
    "Clock",
    "Messaging",
    "Email",
    "Storage",
    "Observability",
    "Persistence"
)

New-CodeDirectories -BasePath $ContractsDirectory -Directories @(
    "Hotels",
    "Guests",
    "Reservations",
    "FrontDesk",
    "Folios",
    "PointOfSale",
    "Inventory",
    "Housekeeping",
    "Maintenance",
    "GuestServices",
    "WifiAccess",
    "Payments",
    "Notifications",
    "Reporting"
)

# Building-block references

Add-ProjectReference `
    -FromProject $ApplicationAbstractionsProject `
    -ToProject $SharedKernelProject

Add-ProjectReference `
    -FromProject $SharedInfrastructureProject `
    -ToProject $ApplicationAbstractionsProject

Add-ProjectReference `
    -FromProject $SharedInfrastructureProject `
    -ToProject $SharedKernelProject

# ------------------------------------------------------------
# Hotel modules
# ------------------------------------------------------------

$Modules = @(
    "Hotels",
    "Identity",
    "Guests",
    "Reservations",
    "FrontDesk",
    "Folios",
    "PointOfSale",
    "Inventory",
    "Housekeeping",
    "Maintenance",
    "GuestServices",
    "WifiAccess",
    "Payments",
    "Notifications",
    "Reporting"
)

$ModuleApiProjects = @()

foreach ($Module in $Modules) {
    Write-Host ""
    Write-Host "Creating module: $Module" -ForegroundColor Cyan

    $ModuleRoot = Join-Path $ModulesRoot $Module

    $DomainName = "$RootNamespace.Modules.$Module.Domain"
    $ApplicationName = "$RootNamespace.Modules.$Module.Application"
    $InfrastructureName = "$RootNamespace.Modules.$Module.Infrastructure"
    $ApiName = "$RootNamespace.Modules.$Module.Api"

    $DomainDirectory = Join-Path $ModuleRoot $DomainName
    $ApplicationDirectory = Join-Path $ModuleRoot $ApplicationName
    $InfrastructureDirectory = Join-Path $ModuleRoot $InfrastructureName
    $ApiDirectory = Join-Path $ModuleRoot $ApiName

    $DomainProject = New-ClassLibraryProject `
        -ProjectName $DomainName `
        -ProjectDirectory $DomainDirectory

    $ApplicationProject = New-ClassLibraryProject `
        -ProjectName $ApplicationName `
        -ProjectDirectory $ApplicationDirectory

    $InfrastructureProject = New-ClassLibraryProject `
        -ProjectName $InfrastructureName `
        -ProjectDirectory $InfrastructureDirectory

    $ApiProject = New-ClassLibraryProject `
        -ProjectName $ApiName `
        -ProjectDirectory $ApiDirectory

    $ModuleApiProjects += $ApiProject

    # Domain folders

    New-CodeDirectories -BasePath $DomainDirectory -Directories @(
        "Entities",
        "ValueObjects",
        "Events",
        "Enums",
        "Errors",
        "Services"
    )

    # Application/CQRS folders

    New-CodeDirectories -BasePath $ApplicationDirectory -Directories @(
        "Abstractions",
        "Commands",
        "Queries",
        "EventHandlers",
        "DTOs",
        "Mappings"
    )

    # Infrastructure folders

    New-CodeDirectories -BasePath $InfrastructureDirectory -Directories @(
        "Persistence",
        "Persistence\Configurations",
        "Persistence\Repositories",
        "Persistence\Migrations",
        "Queries",
        "Services",
        "Integrations"
    )

    # API folders

    New-CodeDirectories -BasePath $ApiDirectory -Directories @(
        "Controllers",
        "Contracts",
        "Authorization",
        "Endpoints"
    )

    # Domain references

    Add-ProjectReference `
        -FromProject $DomainProject `
        -ToProject $SharedKernelProject

    # Application references

    Add-ProjectReference `
        -FromProject $ApplicationProject `
        -ToProject $DomainProject

    Add-ProjectReference `
        -FromProject $ApplicationProject `
        -ToProject $ApplicationAbstractionsProject

    Add-ProjectReference `
        -FromProject $ApplicationProject `
        -ToProject $ContractsProject

    # Infrastructure references

    Add-ProjectReference `
        -FromProject $InfrastructureProject `
        -ToProject $DomainProject

    Add-ProjectReference `
        -FromProject $InfrastructureProject `
        -ToProject $ApplicationProject

    Add-ProjectReference `
        -FromProject $InfrastructureProject `
        -ToProject $SharedInfrastructureProject

    # API references

    Add-ProjectReference `
        -FromProject $ApiProject `
        -ToProject $ApplicationProject

    Add-ProjectReference `
        -FromProject $ApiProject `
        -ToProject $InfrastructureProject

    Add-ProjectReference `
        -FromProject $ApiProject `
        -ToProject $ContractsProject
}

# ------------------------------------------------------------
# Existing HMS.Api Bootstrapper references
# ------------------------------------------------------------

$BootstrapperProject = Join-Path `
    $ProjectRoot `
    "HMS\HMS.Api.csproj"

if (Test-Path $BootstrapperProject) {
    dotnet sln $SolutionPath add $BootstrapperProject

    Add-ProjectReference `
        -FromProject $BootstrapperProject `
        -ToProject $SharedInfrastructureProject

    foreach ($ModuleApiProject in $ModuleApiProjects) {
        Add-ProjectReference `
            -FromProject $BootstrapperProject `
            -ToProject $ModuleApiProject
    }

    $BootstrapperDirectory = Split-Path $BootstrapperProject

    New-CodeDirectories -BasePath $BootstrapperDirectory -Directories @(
        "Authentication",
        "Authorization",
        "Middleware",
        "OpenApi",
        "Extensions"
    )
}
else {
    Write-Warning "Existing HMS.Api project was not found at:"
    Write-Warning $BootstrapperProject
}

# ------------------------------------------------------------
# Test projects
# ------------------------------------------------------------

$ArchitectureTestDirectory =
    Join-Path $TestsRoot "$RootNamespace.ArchitectureTests"

$IntegrationTestDirectory =
    Join-Path $TestsRoot "$RootNamespace.IntegrationTests"

if (-not (Test-Path (Join-Path $ArchitectureTestDirectory "$RootNamespace.ArchitectureTests.csproj"))) {
    dotnet new xunit `
        --name "$RootNamespace.ArchitectureTests" `
        --output $ArchitectureTestDirectory `
        --framework net10.0 `
        --no-restore
}

if (-not (Test-Path (Join-Path $IntegrationTestDirectory "$RootNamespace.IntegrationTests.csproj"))) {
    dotnet new xunit `
        --name "$RootNamespace.IntegrationTests" `
        --output $IntegrationTestDirectory `
        --framework net10.0 `
        --no-restore
}

$ArchitectureTestProject =
    Join-Path $ArchitectureTestDirectory "$RootNamespace.ArchitectureTests.csproj"

$IntegrationTestProject =
    Join-Path $IntegrationTestDirectory "$RootNamespace.IntegrationTests.csproj"

dotnet sln $SolutionPath add $ArchitectureTestProject
dotnet sln $SolutionPath add $IntegrationTestProject

if (Test-Path $BootstrapperProject) {
    Add-ProjectReference `
        -FromProject $IntegrationTestProject `
        -ToProject $BootstrapperProject
}

# ------------------------------------------------------------
# Deployment directories
# ------------------------------------------------------------

New-CodeDirectories -BasePath $DeploymentRoot -Directories @(
    "docker",
    "kubernetes",
    "scripts"
)

# ------------------------------------------------------------
# Restore and build
# ------------------------------------------------------------

Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore $SolutionPath

Write-Host ""
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build $SolutionPath --no-restore

Write-Host ""
Write-Host "HMS Modular Monolith structure created successfully." `
    -ForegroundColor Green