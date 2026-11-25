# Chrono Digger

This is a placeholder(?) repo for the ported code first before I get to actually building the Unity repo.

### Testing

```
cd DotNet~
dotnet test // For single test run
dotnet watch test // For continous test runs
```

### Moving to Unity Repo

Call this command (might need to change library-url) to move all the git files into the place its supposed to be once I start the unity project.

```
git subtree add --prefix=Packages/com.marple.core <library-url> main
```