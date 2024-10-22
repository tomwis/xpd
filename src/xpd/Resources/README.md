## Project Initialization

This project has been initialized with the following features:

- Structure:
```
    Project Root '{solutionName}'
    ├── .config/    # Dotnet tools config folder
    ├── .git/       # New git repo is initialized
    ├── .husky/
    ├── src/
    │   └── Project '{projectName}'
    ├── tests/
    │   └── Test Project '{testProjectName}'
    ├── config/
    │   └── Cache of Husky restore
    ├── build/
    ├── docs/
    ├── samples/
    ├── {solutionName}.sln
    ├── README.md
    ├── .gitignore  # Default .gitignore from dotnet CLI wit hsome additions
    ├── .editorconfig
    ├── Directory.Build.targets
    └── Directory.Packages.props
```
- Solution contains solution folder "SolutionSettings" with files:
  - .gitignore
  - [Directory.Build.targets](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory)
  - [Directory.Packages.props](https://devblogs.microsoft.com/nuget/introducing-central-package-management/)
  - .husky/task-runner.json
- Nuget packages added to test project:
    - [AutoFixture](https://autofixture.github.io)
    - [AutoFixture.AutoNSubstitute](https://github.com/AutoFixture/AutoFixture?tab=readme-ov-file#mocking-libraries)
    - [FluentAssertions](https://fluentassertions.com)
    - Microsoft.NET.Test.Sdk
    - [NSubstitute](https://nsubstitute.github.io)
    - [NSubstitute.Analyzers.CSharp](https://nsubstitute.github.io/help/nsubstitute-analysers/)
    - [NUnit](https://nunit.org)
    - [NUnit3TestAdapter](https://docs.nunit.org/articles/vs-test-adapter/Index.html)
    - [NUnit.Analyzers](https://docs.nunit.org/articles/nunit-analyzers/NUnit-Analyzers.html)
    - [coverlet.collector](https://github.com/coverlet-coverage/coverlet)
- Dotnet tools:
    - [`Husky.Net`](https://alirezanet.github.io/Husky.Net/) is installed and pre-commit hook is initialized
    - [`Csharpier`](https://csharpier.com) is installed and added to pre-commit hook
- Git hooks:
  - [pre-commit] Code formatting of staged files with [Csharpier](https://csharpier.com)
  - [pre-commit] Run solution build
- `Directory.Build.targets` file is created with husky restore (so that developers don't have to do that manually after cloning repo)
- `Directory.Packages.props` file is created and set to manage nuget packages versions
