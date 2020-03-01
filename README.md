# TwitterLite
![Build pipeline status](https://dev.azure.com/jbirdcode/TwitterLite/_apis/build/status/TwitterLite-Docker%20container-CI?branchName=master)


## [Demo Here](https://twitterlite.azurewebsites.net/)

Application that builds twitter-like user feeds using two text files user.txt and tweet.txt. The rendered timeline will be displayed in the console window.

### Usage:
The website will load with the default files initially uploaded. Then new files can be uploaded using the `dropzone`, the files must be named either `user.txt` or `tweet.txt` to allow the system to recognize which file is being uploaded. 

The checkpoints feature is enabled by default but only has real benefits at much greater file sizes. 

If the `user.txt` file is not a continuation of a previous `user.txt` file then please ensure that the toggle is set to **Completely new file** so that the previously created checkpoint files are erased.

All errors are logged to the console.

---

## Assumptions

### Files:
  - The system relies on two files, namely:
    - `user.txt` where each line contains a user followed by the word 'follows' and then a comma seperated list of users they follow.
    - `tweet.txt` where each line contains a user followed by a '>' sign and then a tweet that contains at most 280 characters.
  - The files can only be fully replaced or written to in an append only manner. This allows for a checkpoint point to be created which summarizes the lines already processed. This allows for faster processing in case of resuming from a system crash/restart or uploaded a new file that is a continuation.
  - The files are assumed to be allowed to grow to massive sizes (+2GB) and therefore are read line by line to avoid pulling everything into memory.
  - The files will throw an exception while processing if there is an error with the file. The exception will give detailed information about what went wrong and the area in the file to fix for processing to complete successfully.

#### User File:
  - The user name is allowed to have any special characters and spaces (eg. Vitalik! Buterin follows Kent -> User will be Vitalik! Buterin)

#### Tweet File:
- The users that tweet all exist in the user file and therefore have already been created after processing the user file

#### Architecture:
  - The system is fully self-contained and only relies on Azure Blob Storage for file persistence.
  - The system is memory heavy, keeping all of the users/tweets in memory to deliver high performance in rendering complete timelines
