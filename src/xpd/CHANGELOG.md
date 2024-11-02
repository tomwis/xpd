# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="0.1.0"></a>
## [0.1.0](https://www.github.com/tomwis/xpd/releases/tag/v0.1.0) (2024-11-02)

### Features

* added argument for init command to provide solution name ([9efa8d1](https://www.github.com/tomwis/xpd/commit/9efa8d12ba2798d7f0d499ccdac30b22ce172b41))
* added creation of default folders in init command ([5525520](https://www.github.com/tomwis/xpd/commit/5525520e3ea903e1b7ba861846a30170523b3fd1))
* added creation of Directory.Build.targets in init command ([f3d1ba5](https://www.github.com/tomwis/xpd/commit/f3d1ba5ddebca9e9dcc0dd024245d660386cd228))
* added creation of Directory.Packages.props in init command ([93cbc8b](https://www.github.com/tomwis/xpd/commit/93cbc8b42940421b22f9c3142300861b3c625157))
* added creation of test project and adding it to solution in init command ([cc8580b](https://www.github.com/tomwis/xpd/commit/cc8580b8d9b16140742f84b6270bd50210bcbe31))
* added generating dotnet tools manifest and installing csharpier in init command ([3e80adc](https://www.github.com/tomwis/xpd/commit/3e80adc0d14e24974fb05235a49e83f0bf0ebbbf))
* added handling errors from external processes ([edf9f90](https://www.github.com/tomwis/xpd/commit/edf9f9070b91f9b4c1bad1b5c14a9e71d4a65197))
* added husky and pre-commit hook to run csharpier in init command ([b99f4ef](https://www.github.com/tomwis/xpd/commit/b99f4efdcea12730f6b5cf4b44c7ccacea36ab9b))
* added Husky restore logic in Directory.Build.targets in init command ([773553e](https://www.github.com/tomwis/xpd/commit/773553eb00d103838b62762f785fd3b920a8908a))
* added informational messages for commit message checks ([a6ceb04](https://www.github.com/tomwis/xpd/commit/a6ceb042997e2bff4f77e1132d7de3cbe335205c))
* added init command with generation of solution and project ([17fe053](https://www.github.com/tomwis/xpd/commit/17fe053779a8c897fd168d148b614006d7dd7362))
* added initializing git repository in init command ([53ea7a7](https://www.github.com/tomwis/xpd/commit/53ea7a7561af22cbc21c3ad9c34664bfafe0975e))
* added more nugets to test project ([2f7f724](https://www.github.com/tomwis/xpd/commit/2f7f72464ca04601bfaf773690415d0cd5147756))
* creating solution always in main folder ([578930f](https://www.github.com/tomwis/xpd/commit/578930f4357474ab8a93bdf20b8f8b6550d88693))
* removed input for project name and folders to simplify process in init command ([4159bba](https://www.github.com/tomwis/xpd/commit/4159bbab2315d56ccc718a09b737318010640bde))
* using English in dotnet CLI ([1b92cf1](https://www.github.com/tomwis/xpd/commit/1b92cf1d779286c93066da241d99854bc10da755))
* **init:** added --no-build flag to git hook test run in init command ([c724e86](https://www.github.com/tomwis/xpd/commit/c724e869001b61eba5b4044dfcff06d2864e5964))
* **init:** added .gitignore file to main folder in init command ([91e7413](https://www.github.com/tomwis/xpd/commit/91e74138e092aa33ffcc71fd2cbbd892131f5564))
* **init:** added .gitignore to SolutionSettings solution folder in init command ([5b4e0b7](https://www.github.com/tomwis/xpd/commit/5b4e0b75056158f9464d1953e7322affcfe285a5))
* **init:** added check for integration tests init command ([a3c7089](https://www.github.com/tomwis/xpd/commit/a3c70895904fc8553b585131b03fff417b23f6b1))
* **init:** added configuration of project type in init command ([58cc768](https://www.github.com/tomwis/xpd/commit/58cc768493508dba68ddc28ec7f3dd0a18f82577))
* **init:** added convention tests project in init command ([5f73273](https://www.github.com/tomwis/xpd/commit/5f732733877166ea95ee57266d601b26a7cabd71))
* **init:** added default folders to tests project in init command ([87b5eab](https://www.github.com/tomwis/xpd/commit/87b5eab22deba79261283f07f27f9f306d7f61df))
* **init:** added more project types in init command ([c8a2d22](https://www.github.com/tomwis/xpd/commit/c8a2d226a71c4b439e736e746423767ce0377cf6))
* **init:** added pre-commit convention test run in init command ([14b7048](https://www.github.com/tomwis/xpd/commit/14b7048b1a64d7e16cdf1725c1f325eeda54c68c))
* **init:** added README.md to mainFolder in init command ([272afc4](https://www.github.com/tomwis/xpd/commit/272afc4c0f675e4c53d52e71dae8bd9f22f5ffcb))
* **init:** added solution build pre-commit hook to init command ([0720acd](https://www.github.com/tomwis/xpd/commit/0720acdba6d250c844781be7543cafa9ceea6efb))
* **init:** added solution folder with items in init command ([beff8ca](https://www.github.com/tomwis/xpd/commit/beff8caed3abb28a141db97369bf8dd4a107f443))
* **init:** added unit tests run pre-commit hook to init command ([a5c4235](https://www.github.com/tomwis/xpd/commit/a5c423520ad235db37167112561cb7778993236f))
* **init:** adding .editorconfig to main folder and solution folder in init command ([5f5af8e](https://www.github.com/tomwis/xpd/commit/5f5af8efdda048e95ea6f38009826d893975b094))
* **init:** updated README.md ([3707df2](https://www.github.com/tomwis/xpd/commit/3707df2ee93b3f096ef00d4cb676b2d3b8149b36))

### Bug Fixes

* added schema to TaskRunner class ([ab5cb6c](https://www.github.com/tomwis/xpd/commit/ab5cb6c0edb7a100c856264569c805399a7b5e7d))
* fixed warning ([f401540](https://www.github.com/tomwis/xpd/commit/f401540ac899a98f2f6bccaced0c13b2546141fc))
* moving package versions to Directory.Packages.props from csproj in init command ([9973267](https://www.github.com/tomwis/xpd/commit/9973267f6f95ae397342dcf798d149872d2d3152))
* removed slash after DirectoryBuildTargetsDir as it is already there ([17486ec](https://www.github.com/tomwis/xpd/commit/17486ec0f01e9676c97abac6e32a8a0b176737d9))
* **init:** added a fix for pre-commit tests run in init command ([10dd50c](https://www.github.com/tomwis/xpd/commit/10dd50c57bc567246fe7cad9f02e8280edba4b85))

