const express = require("express");
const bodyParser = require("body-parser");
const path = require("path");
const fs = require("fs");

const app = express();

app.use(bodyParser.json());

const PORT = 3000;

const TELEMETRY_PATH = path.join(__dirname, "events.json");
const SAVEDATA_PATH = path.join(__dirname, "cloudsaves.json");

// Endpoint to handle telemetry data
app.post('/telemetry', (req, res) => {
    try {
        const eventData = req.body;

        let existingEvents = [];
        if (fs.existsSync(TELEMETRY_PATH)) {
            const rawData = fs.readFileSync(TELEMETRY_PATH, "utf-8");
            if (rawData.length > 0) {
                existingEvents = JSON.parse(rawData); // Corrected assignment
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