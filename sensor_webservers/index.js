const express = require('express');
const app = express();

//read config.json file
const fs = require('fs');
const config = JSON.parse(fs.readFileSync('config.json', 'utf8'));
const BASE_PORT = 10000;

function humidity(req, res) {
    //return random value
    var value = Math.random() * 100;
    res.send(value.toString());
}

function temperature(req, res) {
    //return random value
    var value = Math.random() * 45;
    res.send(value.toString());
}

function callbackFunction(config, i) {
    return function() {
        var port = BASE_PORT + i;
    var webServer = express();
    var terrainName = config.name;
    var sensors = config.sensor;
    for(var j=0;j<sensors.length;j++) {
        var sensor = sensors[j];
        var sensorName = sensor.name;
        if(sensor.type == 'humidity') {
            webServer.get(`/terrain/${terrainName}/sensor/${sensorName}`, humidity);
        } else if(sensor.type == 'temperature') {
            webServer.get(`/terrain/${terrainName}/sensor/${sensorName}`, temperature);
        }
    }
    webServer.get(`/terrain/${terrainName}/actuator`, function(req, res) {
        var actuatorActive = webServers[i].actuatorActive;
        res.send(actuatorActive.toString());
    });
    webServer.post(`/terrain/${terrainName}/actuator/:active`, function(req, res) {
        var active = req.params.active;
        webServers[i].actuatorActive = active;
        res.send('ok');
    })
    webServer.use(express.static('public'));
    webServer.listen(port, function() {
        console.log('web server for ' + terrainName + ' is listening on port ' + port);
    });
    }
}

var webServers = [];
for(var i=0;i<config.length;i++) {
    webServers.push({
        actuatorActive: false,
        webServer: callbackFunction(config[i], i)()
    });
}