# RTL.API

RTL.API is an Service that scrapes data from https://www.tvmaze.com/api and stores in MongoDB and provides an API for getting show and cast info in a paginated manner

### Prerequisites 

You must have docker set up in your system 

## Installation

On the project folder (RTL.API) run this command
```
docker-compose up
```

## Usage

When Docker container is up background service will start scraping data this can be queried by Mongo express from http://localhost:8081/  
Also API is accessible from http://localhost:5000/index.html
