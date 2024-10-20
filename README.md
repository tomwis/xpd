# What is xpd?

Xpd is a tool for initializing new dotnet project with convenient defaults. There are a lot of things that we often want to configure when we set up a new project. This tool is there to help us do it in seconds instead of hours or days.

# What is configured?
- New folder for whole solution is created
- New solution is created
- New console project is created
- New test project is created with basic nuget packages
- Folders in root repo folder are created: src, tests, samples, docs, build, config
- Git repository is initialized
- Some dotnet tools are set up:
  - Husky.Net is installed and pre-commit hook is initialized
  - Csharpier is installed and added to pre-commit hook
- Directory.Build.targets file is created with husky restore (so that developers don't have to do that manually after cloning repo)
- Directory.Packages.props file is created and set to manage nuget packages versions

# How to use it?
For now, download the repo and run in src/xpd folder:

`dotnet run init "MySolutionName"`

You can also run:

`dotnet run init  "MySolutionName" --output "path/to/output/folder"`

If you'd like to place your solution somewhere else than current directory (which you should do if you run it from source).