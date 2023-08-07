# TDDD27_2023_scribbly

## Functional specification
Scribbly is a website that allows users to play a word guessing game. One user hosts the game in which there are a few rounds. Players can join the game via the invitation URL. In each round each player has a chance to draw an image presenting the randomly chosen word. Players write their guesses on the chat and whoever guesses the word correctly receives points. The player with the highest score after all rounds wins. 

Host is able to set:
+ number of rounds
+ type of random words
+ timespan of each round

## Technological specification

`SignalR` for managing realtime client-server communication

#Frontend
+ `React` with `Typescript` (built with Vite)
+ `Bootstrap` library for styling

#Backend
+ `ASP.NET Core` (REST API)
+ `Sqlite` database
+ `Dapper` ORM framework
+ `Wordnik API` to fetch random word

## Screenshots
Main menu
![Screenshot 2023-08-07 202112](https://github.com/hynas321/Scribbly/assets/76520333/b875f960-bd61-46db-86b1-d577020d7d2c)

Game lobby
![Screenshot 2023-08-07 202202](https://github.com/hynas321/Scribbly/assets/76520333/7b526b2f-7a86-4cdb-9413-ca7fe76c2aec)

Drawing view
![Screenshot 2023-08-07 202314](https://github.com/hynas321/Scribbly/assets/76520333/3d58487e-e886-49dd-9fcd-169fbb7c55c0)

Guessing view
![Screenshot 2023-08-07 202408](https://github.com/hynas321/Scribbly/assets/76520333/6dd0257d-5897-463f-b238-167ce9ac2392)





