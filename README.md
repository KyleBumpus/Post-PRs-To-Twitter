# PostPRsToTwitter
Gets latest pull requests from a Github repo and Tweets the details.

github repo:
https://github.com/KyleBumpus/dummy-repo

Twitter account:
https://twitter.com/DummyAc58005337

# Twitter API secrets
The Twitter API requires the use of secrets, which we will read from an environment variable for simplicity.
In a production environment, we would store the secrets in something like Azure KeyVault or AWS Secrets Manager.
You will need to create environment variables with the secrets before running the app.

The way you set environment variables varies depending on which shell you're using. For Powershell on Windows, it is:


$env:TwitterApiKey="<API_KEY>"

$env:TwitterApiKeySecret="<API_KEY_SECRET>"

$env:TwitterAccessToken="<ACCESS_TOKEN>"

$env:TwitterAccessTokenSecret="<ACCESS_TOKEN_SECRET>"


The program will only have access to environment variables from the environment block of the current process. 
See Microsoft's docs for details on what that means for your platform.
https://docs.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable?view=netcore-2.2

# Usage
Navigate to the .\UpdateTwitterStatus folder (where the UpdateTwitterStatus.csproj folder is located).
Run "dotnet run"

State is persisted in RunHistory.xml. It keeps track of all the PRs you've successfully tweeted and prevents duplicate tweets. 
If you delete this file, the code will think nothing has ever between tweeted and behave accordingly. In production, this would 
be in a database rather than a simple xml file.

# Tests
Navigate to the .\UpdateTwitterStatus.Tests folder (where the UpdateTwitterStatus.Tests.csproj folder is located).
Run "dotnet test"

A note on test coverage:
I had planned to mock out the PostAsync call in TwitterHttpClient as I had done for SendAsync in the GithubHttpClient, but it turns
out PostAsync is not virtual and thus not overridable like SendAsync is. If I had more time, I would wrap HttpClient, consume it in both 
clients, and mock that instead.
