# CourseWork
## Requirements
Project made using .NET Core 7
## Installing
For install download zip of this repo OR
use `git clone https://github.com/Snakemonster/CourseWork.git`
in folder where you want.
## Building
### For Visual Studio/Rider
After installing repo 
unzip files.rar in folder with projects like on image below:

![img.png](files.png)

Then open solution in your IDE. For running project you need to 
build and run Server, then ClientExmp.
### For CLI 
For this option you still should unzip file.rar but you can 
unzip it anywhere you want (you should remember path to it). 
Then you need to be in folder where project and run command:


`dotnet build
`

After that you need two command line:
1st for server and 2nd for client.

For Server on 1st command line:

`cd Server ; dotnet run *path to unziped "files" folder*
`

For Client on 2nd command line

`cd ClientExmp ; dotnet run
`

### Note
Project Client is library, so it doesn't have 
function main and can't generate executable. 