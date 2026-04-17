# Stakes

<p align="center">
  <img src="https://github.com/rodrigo-pariente/stakes/blob/main/images/logo.png" alt="Stakes logo"/>
  <b>Cli habit tracker, simple and pretty, written in C#.</b>
</p>


## How To Use

You can start to track a habit using:

`$ Stakes habit add gym`

And log your efforts:

`$ Stakes commit add gym 1h -m "Hard work. Leg day."`

Add an encouraging message:

`$ Stakes quote gym 'No pain, no gain'`

Get some motivation with:

`$ Stakes courage gym`

At the end of the day, see your report:

`$ Stakes report`

<img src="https://github.com/rodrigo-pariente/stakes/blob/main/images/stakes-report.png" alt="Stakes beautiful habit tracking report"/>

See more of the **Stakes** capabilities with:

`$ Stakes --help`

> [!NOTE]
> Stakes uses SQLite for its databases, allowing it to have a fast, extensible and reliable storage.


## Compiling From Source

Stakes depends on the following nuget packages:

- Microsoft.Data.Sqlite
- System.CommandLine
- Pastel


### Linux

Requires the **.NET SDK** installed on your system.

```bash
git clone https://github.com/rodrigo-pariente/Stakes
cd Stakes/scripts/
./build
./install # may require sudo privileges
```


## Motivation

This program was designed mainly for me to learn and improve my C# and SQLite skills.

It focuses on simplicity, meaningful feedback, and a clean CLI experience.
