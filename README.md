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
![Screenshot 2023-08-07 202112](https://github.com/hynas321/Scribbly/assets/76520333/30019fad-e550-41be-8a5e-465b9266b8b7)

Game lobby
![Screenshot 2023-08-07 202202](https://github.com/hynas321/Scribbly/assets/76520333/8a7c3812-2f22-4302-b294-2fc9aa5c8968)

Drawing view
![Screenshot 2023-08-07 202314](https://github.com/hynas321/Scribbly/assets/76520333/45956045-dea3-4652-b94a-6376702f628c)

Guessing view
![Screenshot 2023-08-07 202408](https://github.com/hynas321/Scribbly/assets/76520333/f905b489-65af-4b24-95ea-4b78ce1965c0)




