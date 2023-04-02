# TDDD27_2023_scribbly

## Functional specification
Scribbly is a website that allows users to play a word guessing game. One user hosts the game in which there are a few rounds. Players can join the game via the invitation URL. In each round each player has a chance to draw an image presenting the randomly chosen word. Players write their guesses on the chat and whoever guesses the word correctly receives points. The player with the highest score after all rounds wins. 

Host is able to set:
+ number of rounds
+ types/categories of random words
+ timespan of each round

## Technological specification

Websockets are used for the client-server communication.

#Frontend
+ `React` with `Typescript` (built with Vite)
+ `Bootstrap` library for styling

#Backend
+ `ASP.NET Core Web API` (REST API)
+ `MS SQL Server` to manage number of rounds, player's score etc.
+ `Dapper` ORM framework
+ `Wordnik API` to fetch random word
