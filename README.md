# Scribbly

## Description
**Scribbly** is an online platform designed for engaging in a word-guessing game. On this platform, a designated user serves as the game host, and the game is usually organized into multiple rounds. 

To participate, players can enter the game by using an invitation link. During each round, every player is given an opportunity to create a visual representation of a randomly selected word. The other participants then submit their word guesses in the chat, and the player who correctly identifies the word earns points. 

The player with the highest score at the end of all the rounds wins.

## Technological specification

### Frontend
+ **React** with **Typescript** (built with Vite)
#### NPM packages:
+ **Bootstrap** - styling and responsive design
+ **Redux** - state management
+ **Ajax** - HTTP communication
+ **use-local-storage-state** - used to keep access tokens for OAuth 2.0 and game tokens which determine permissions related to lobby host and drawing on canvas


### Backend
+ **ASP.NET Core** (REST API)
+ **Sqlite** database
+ **Dapper** ORM framework
+ **Wordnik API** to fetch a random word

**SignalR** for managing realtime client-server communication

## Running the website
React app: `npm install` `npm run dev`
Server: `dotnet run`

## Screenshots

#### Main menu
![screenshot1](https://github.com/hynas321/Scribbly/assets/76520333/b41caab8-ce0f-4c4d-9b96-765cbdbb3737)

#### Main menu - pop-up
![screenshot2](https://github.com/hynas321/Scribbly/assets/76520333/7c17d624-f0fd-4fb0-9ca7-b259050412ed)

#### Lobby
![screenshot3](https://github.com/hynas321/Scribbly/assets/76520333/a013b445-89f5-4e16-a058-ee8c3f98bf28)

#### Game - drawing
![screenshot4](https://github.com/hynas321/Scribbly/assets/76520333/e735a809-3186-4b60-bdd6-39e5eb05046c)

#### Game - drawing phase ended
![screenshot5](https://github.com/hynas321/Scribbly/assets/76520333/60886dc8-4a74-4e14-b479-d954b106125a)

#### Responsive design - example
![Screenshot 2023-10-17 235554](https://github.com/hynas321/Scribbly/assets/76520333/4742da73-6230-450b-a91c-4c6f4f298a6e)
