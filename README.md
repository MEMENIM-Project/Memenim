# <div align="center">**M E M E N I M**</div>


## <div align="center">[![LatestRelease](https://img.shields.io/github/v/release/MEMENIM-Project/Memenim?style=flat&color=MEMENIM&label=latest%20release)](https://github.com/MEMENIM-Project/Memenim/releases/latest) [![CodeLines](https://tokei.rs/b1/github/MEMENIM-Project/Memenim?category=code)](https://github.com/MEMENIM-Project/Memenim) [![ClosedMilestones](https://img.shields.io/github/milestones/closed/MEMENIM-Project/Memenim?style=flat)](https://github.com/MEMENIM-Project/Memenim/milestones?state=closed) [![ClosedIssues](https://img.shields.io/github/issues-closed/MEMENIM-Project/Memenim?style=flat)](https://github.com/MEMENIM-Project/Memenim/issues?q=is%3Aissue+is%3Aclosed)</div>


## <div align="center">[![Discord](https://img.shields.io/discord/758775270861307946?style=flat&label=discord&logo=discord&logoColor=8099DF&color=5194f0)](https://discord.gg/yhATVBWxZG) [![Telegram](https://img.shields.io/badge/telegram-MEMENIM%20%20Ghetto-2A8?style=flat&label=telegram&logo=telegram&logoColor=white&color=5194f0)](https://t.me/joinchat/Vf9B3XM5SM-zUbkf)</div>


### <div align="center">Custom client for [Anonym social network](https://anonym.network/).</div>


<br/>


# <div align="center">**Information**</div>


## Questions, bug reports or feature requests?

Do you have any questions? [Create an issue](https://github.com/MEMENIM-Project/Memenim/issues/new/choose).

Are you missing some features or have you found bugs? [Create an issue](https://github.com/MEMENIM-Project/Memenim/issues/new/choose) or even better, [contribute to MEMENIM](https://github.com/MEMENIM-Project/Memenim#Contributing)!


## Frequently Asked Questions (FAQ)

See [FAQ](https://github.com/MEMENIM-Project/Memenim/wiki/FAQ) on the Wiki.


## Contributing

As the current MEMENIM team is a small team (currently it consists of only one person), we cannot fix every bug or implement every feature on our own. So contributions are really appreciated!

A good way to get started (flow):

1. Fork the MEMENIM repository.
2. Create a new branch in you current repository from the 'master' branch.
3. 'Check out' the code with Git or [GitHub Desktop](https://desktop.github.com/).
4. Push commits and create a Pull Request (PR) to MEMENIM.


## License

MEMENIM is open source software, licensed under the terms of MIT license.
See [LICENSE](LICENSE) for details.


## How to build

Visual Studio (2019 or newer):

  - Open the solution 'Memenim.sln'.
  - Change the configuration to 'Release'.
  - Click on the 'Build' button.
  - Go to the '\bin\Release\netcoreapp3.1' directory.

Dotnet CLI tool:

  - Open the terminal.
  - Go to the solution directory.
  - Enter the 'dotnet build -c "Release" "Memenim.sln"' command.
  - Go to the '\bin\Release\netcoreapp3.1' directory.


## How to publish

Dotnet CLI tool:

  - Go to the solution directory.
  - Open the file 'Publish.bat'.
  - Wait for the progress to be completed.
  - Go to the '\bin\Release\netcoreapp3.1\publish' directory.
  - Select a release by directory name *Platform*-*DeploymentType*{-*OptionalParameter*}, where:
    - *Platform* - **win-x64** or **win-x86** (RID's - Runtime IDentifier's).
    - *DeploymentType* - **FDD** (Framework-Dependent Deployment) or **SCD** (Self-Contained Deployment (Standalone)).
    - *OptionalParameter* - extra options (multiple and optional), such as:
      - **nosingle** - disables packing the app into a single file.


