const express = require("express");
const bodyParser = require("body-parser");
const path = require("path");
const fs = require("fs");

const app = express();

app.use(bodyParser.json());

const PORT = 3000;

const PATH = path.join(__dirname, "events.json")

app.post('/telemetry', (req, res) => {
    try {
        const eventData = req.body;

        let existingEvents = [];
        if (fs.existsSync(PATH)) {
            const rawData = fs.readFileSync(PATH, "utf-8");
            if (rawData.length > 0) {
                existingEvents - JSON.parse(rawData);
            }
        }

        eventData.timestamp = new Date().toISOString();
        existingEvents.push(eventData);

        fs.writeFileSync(PATH, JSON.stringify(existingEvents, null, 2));

        return res.status(200).json({ message: "Data Stored" });
    } catch (error) {
        return res.status(500).json({ error: "Error" });
    }
});

app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`)
});