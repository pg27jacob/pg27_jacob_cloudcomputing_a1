const express = require("express");
const bodyParser = require("body-parser");
const path = require("path");
const fs = require("fs");
const jwt = require("jsonwebtoken"); // Import jsonwebtoken

const app = express();

app.use(bodyParser.json());

const PORT = 3000;

const TELEMETRY_PATH = path.join(__dirname, "events.json");
const SAVEDATA_PATH = path.join(__dirname, "cloudsaves.json");

// Replace with database (for demonstration purposes)
const users = {
    'player1': { password: 'password1', id: 1 },
};

// Secret key (Store this securely! Upadate to work properly later)
const SECRET_KEY = 'YourReallySecureKeyHere';

// 1. Authentication Route (/login)
app.post('/login', (req, res) => {
    const { username, password } = req.body;

    if (!username || !password) {
        return res.status(400).json({ error: 'Username and password are required' });
    }

    if (users[username] && users[username].password === password) {
        // User authenticated successfully
        const user = users[username];
        const payload = {
            userId: user.id,
            username: username,
            exp: Math.floor(Date.now() / 1000) + (60 * 60), // Token expires in 1 hour (in seconds)
        };

        // Generate the JWT
        const token = jwt.sign(payload, SECRET_KEY);

        // Send the token back to the client
        res.json({ token });
    } else {
        // Authentication failed
        res.status(401).json({ error: 'Invalid credentials' });
    }
});

// Endpoint to handle telemetry data
app.post('/telemetry', (req, res) => {
    try {
        const eventData = req.body;

        let existingEvents = [];
        if (fs.existsSync(TELEMETRY_PATH)) {
            const rawData = fs.readFileSync(TELEMETRY_PATH, "utf-8");
            if (rawData.length > 0) {
                existingEvents = JSON.parse(rawData);
            }
        }

        eventData.timestamp = new Date().toISOString();
        existingEvents.push(eventData);

        fs.writeFileSync(TELEMETRY_PATH, JSON.stringify(existingEvents, null, 2));

        return res.status(200).json({ message: "Telemetry Data Stored" });
    } catch (error) {
        console.error("Error storing telemetry data:", error);
        return res.status(500).json({ error: "Error storing telemetry data" });
    }
});

// Endpoint to handle saving game data
app.post('/savegame', (req, res) => {
    try {
        const saveData = req.body;

        fs.writeFileSync(SAVEDATA_PATH, JSON.stringify(saveData, null, 2));

        return res.status(200).json({ message: "Game Saved" });
    } catch (error) {
        console.error("Error saving game data:", error);
        return res.status(500).json({ error: "Error saving game data" });
    }
});

// Endpoint to load game data
app.get('/loadgame', (req, res) => {
    try {
        if (fs.existsSync(SAVEDATA_PATH)) {
            const rawData = fs.readFileSync(SAVEDATA_PATH, "utf-8");
            const saveData = JSON.parse(rawData);
            return res.status(200).json(saveData);
        } else {
            return res.status(404).json({ message: "No save data found" });
        }
    } catch (error) {
        console.error("Error loading game data:", error);
        return res.status(500).json({ error: "Error loading game data" });
    }
});

app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});
