## Project Initialization

This project has been initialized with the following features:

- Structure:
```
    Project Root '{solutionName}'
    ├── .config/    # Dotnet tools config folder
    ├── .git/       # New git repo is initialized
    ├── .husky/
    ├── src/
    │   └── Console Project '{projectName}'
    ├── tests/
    │   └── Test Project '{testProjectName}'
    ├── config/
    │   └── Cache of Husky restore
    ├── build/
    ├── docs/
    ├── samples/
    ├── {solutionName}.sln
    ├── README.md
    ├── Directory.Build.targets
    └── Directory.Packages.props
```
- Nuget packages added to test project:
    - AutoFixture
    - AutoFixture.AutoNSubstitute
    - FluentAssertions
    - Microsoft.NET.Test.Sdk
    - NSubstitute
    - NSubstitute.Analyzers.CSharp
    - NUnit
    - NUnit3TestAdapter
    - NUnit.Analyzers
    - coverlet.collector
- Dotnet tools:
    - `Husky.Net` is installed and pre-commit hook is initialized
    - `Csharpier` is installed and added to pre-commit hook
- `Directory.Build.targets` file is created with husky restore (so that developers don't have to do that manually after cloning repo)
- `Directory.Packages.props` file is created and set to manage nuget packages versions
