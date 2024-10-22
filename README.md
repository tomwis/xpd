# What is xpd?

Xpd is a tool for initializing new dotnet project with convenient defaults. There are a lot of things that we often want to configure when we set up a new project. This tool is there to help us do it in seconds instead of hours or days.

# What is configured?

Basic project with a structure, tools and packages that may be helpful. Go [here](src/xpd/Resources/README.md) for details.

# How to use it?
For now, download the repo and run in src/xpd folder:

`dotnet run -- init "MySolutionName"`

You can also run:

`dotnet run -- init "MySolutionName" --project-type "maui" --output "path/to/output/folder"`

If you'd like to place your solution somewhere else than current directory (which you should do if you run it from source) or select other project type than default console project (type init --help to see all supported project types).