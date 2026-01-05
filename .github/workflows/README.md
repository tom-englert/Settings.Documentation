# GitHub Actions Workflows

## Build and Publish Workflow

### Overview
This workflow builds the solution, runs tests, and publishes NuGet packages.

### Triggers
- **Push to main**: Builds and tests only
- **Pull requests**: Builds and tests only (no publishing)
- **Tags (v*)**: Builds, tests, and publishes packages to NuGet.org
- **Manual**: Can be triggered manually via workflow dispatch

### Jobs

#### 1. Build and Test
- Restores dependencies
- Builds the solution in Release configuration
- Runs all tests in the solution
- Packs NuGet packages
- Uploads packages as artifacts

#### 2. Publish
- Downloads the built packages
- Publishes to NuGet.org (only when version tags are pushed)
- Only runs for tags starting with 'v' (e.g., v1.0.0)

### Required Secrets
- `NUGET_API_KEY`: Your NuGet.org API key for publishing packages

### Published Packages
1. `TomsToolbox.Settings.Documentation.Abstractions`
2. `TomsToolbox.Settings.Documentation.Builder`
3. `TomsToolbox.Settings.Documentation.Analyzer`

### Setup Instructions
1. Generate a NuGet API key at https://www.nuget.org/account/apikeys
2. Add the API key as a secret in your GitHub repository:
   - Go to Settings > Secrets and variables > Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key
3. Create a version tag to trigger publishing

### Version Management
Versions are controlled by the `<Version>` property in `Directory.build.props`.
- Update the version in `Directory.build.props` (e.g., `1.0.0` or `1.0.0-beta1`)
- Create and push a tag with the `v` prefix (e.g., `v1.0.0`) to trigger publishing
- Only tagged versions are published to NuGet.org
