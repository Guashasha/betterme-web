const express = require('express');
const app = express();

//TODO: add database connection

app.use(express.json())
//TODO: import routes

app.listen(3000, () => {
    console.log('UsersService running in http://localhost:3000')
});