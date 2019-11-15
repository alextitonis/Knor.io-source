//"use strict";

var port = process.env.PORT || 7777;
var io = require('socket.io')(port);
var shortId = require('shortid');

var _maxHealth = 100;

class Position {
	constructor(x, y, z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	getX() { return this.x; }
	getY() { return this.y; }
	getZ() { return this.z; }
	
	setX(x) { this.x = x; }
	setY(y) { this.y = y; }
	setZ(z) { this.z = z; }
}
class Box {
	constructor(minX, minY, minZ, maxX, maxY, maxZ, centerX, centerY, centerZ)
	{
		this.minX = minX
		this.minY = minY
		this.minZ = minZ
		
		this.maxX = maxX
		this.maxY = maxY
		this.maxZ = maxZ
		
		this.centerX = centerX;
		this.centerY = centerY;
		this.centerZ = centerZ;
	}
}
class Zone {
	constructor(id, box, _players, ownerID) 
	{
		this.id = id;
		this.box = box;
		this._players = _players;
		this.ownerID = ownerID;
		
		this._timeValue = false;
		
		this._checkerThread = setInterval(() => {
			for(var p in _players) 
			{
				if (_players[p] != null && _players[p] != undefined)		   
					if (!isPointInsideAABB(new Position(players[p].posX, players[p].posY, players[p].posZ), this.box))
						delete _players[p];
			}
		}, secondsToMiliseconds(60));
	}
	
	setOwner(ownerID) 
	{
		this.ownerID = ownerID;
		
		if (this._timeValue)
			clearInterval(this._timer);
		
		this._timeValue = true;
		this._timer = setInterval(() => {
			switch (this.ownerID) 
			{
				case 0:
				warriorScore += zonePoints;
				break;
				case 1:
				orcScore += zonePoints;
				break;
			}
			
			if (this.ownerID == 0 || this.ownerID == 1)
				io.sockets.emit('score', {k1:warriorScore, k2:orcScore});
		
		}, secondsToMiliseconds(timeToScoreZonePoints));
	}		
}
class Player {
	constructor(id, name, isBot, posX, posY, posZ, rotX, rotY, rotZ, rotW, maxHealth, currentHealth, charId, zoneID)
	{
		this.id = id;
		this.name = name;
		this.isBot = isBot;
		
		this.posX = posX;
		this.posY = posY;
		this.posZ = posZ;
		
		this.rotX = rotX;
		this.rotY = rotY;
		this.rotZ = rotZ;
		this.rotW = rotW;
		
		this.maxHealth = maxHealth;
		this.currentHealth = currentHealth;
		
		this.charId = charId;
	}
}
class ZoneData {
	constructor(id, ownerID) 
	{
		this.id = id;
		this.ownerID = ownerID;
	}
}
class InitialZoneData {
	constructor(id, centerX, centerY, centerZ, ownerID)
	{
		this.id = id;
		
		this.centerX = centerX;
		this.centerY = centerY;
		this.centerZ = centerZ;
		
		this.ownerID = ownerID;
	}
}

var players = [];
var clients = [];
var currentPlayers = 0;
var maxPlayers = 100;

var warriorScore = 0;
var orcScore = 0;

var killPoints = 1;
var zonePoints = 10;
var timeToScoreZonePoints = 30; //in seconds
 
var zones = [];
zones[1] = new Zone(1, new Box(-66.35, -37.06, 68.83, -42.21, -20.64, 94.22, -54.43, -29.5, 79.08), [], -1);
zones[2] = new Zone(2, new Box(-165.06, -41.58, 62.6, -141.29, -28.02, 87.46, -154.06, -36, 74), [], -1);
zones[3] = new Zone(3, new Box(-223.99, -41.24, 13.42,-198.31 , -25.56, 39.84, -212.06, -33.215, 27.51), [], -1);
zones[4] = new Zone(4, new Box(-293.89, -34.81, -0.33, -270, -18.5474, 23.36, -282.48, -29.22, 10.52), [], -1);
zones[5] = new Zone(5, new Box(-377.11, -47.39, 47.82, -356.37, -12.95, 72.47, -366.58, -31, 59.28), [], -1);

console.log("server started on port " + port);

function secondsToMiliseconds(seconds) {
	return seconds * 1000;
}
function getRandomInt(max) {
  return Math.floor(Math.random() * Math.floor(max));
}
function getSpawn(charID) {
	var spawnPosition = getRandomInt(4);
	var pos = new Position(0, 0, 0);
	
	if (charID == 0)
	{
		//Warior
		if (spawnPosition == 0)
		{
			pos.setX(-69.52);
			pos.setY(-22.2142);
			pos.setZ(-4.09);
		}
		else if (spawnPosition == 1)
		{
			pos.setX(-58.58);
			pos.setY(-22.2142);
			pos.setZ(-5.47);
		}
		else if (spawnPosition == 2)
		{
			pos.setX(-64.58);
			pos.setY(-22.2142);
			pos.setZ(0.63);
		}
		else if (spawnPosition == 3)
		{
			pos.setX(-65.41);
			pos.setY(-22.2142);
			pos.setZ(-6.49);
		}
		else if (spawnPosition == 4)
		{
			pos.setX(-69.74);
			pos.setY(-22.2142);
			pos.setZ(2.27);
		}
	} 
	else
	{
		//Orc
		if (spawnPosition == 0)
		{
			pos.setX(-308.2316);
			pos.setY(-22.2142);
			pos.setZ(125.0595);
		}
		else if (spawnPosition == 1)
		{
			pos.setX(-312.94);
			pos.setY(-22.2142);
			pos.setZ(131.54);
		}
		else if (spawnPosition == 2)
		{
			pos.setX(-301.27);
			pos.setY(-22.2142);
			pos.setZ(126.41);
		}
		else if (spawnPosition == 3)
		{
			pos.setX(-307.33);
			pos.setY(-22.2142);
			pos.setZ(136.54);
		}
		else if (spawnPosition == 4)
		{
			pos.setX(-298.06);
			pos.setY(-22.2142);
			pos.setZ(133.36);
		}
	}
	
	return pos;
}
function isPointInsideAABB(point, box) {
  return (point.x >= box.minX && point.x <= box.maxX) &&
         (point.y >= box.minY && point.y <= box.maxY) &&
         (point.z >= box.minZ && point.z <= box.maxZ);
}
function count(obj) {
    var count = 0;
    for (var p in obj) {
      obj.hasOwnProperty(p) && count++;
    }
    return count; 
}
function getRandomPlayerFromZone(_players) {
	for (var p in _players)
		return p;
}
function getRandomPlayer() {
	for (var p in players)
		return p;
}
function zoneCheck(data, id) {
	var pos = new Position(data.x, data.y, data.z);
	
	for(var z in zones) 
	{
		if (isPointInsideAABB(pos, zones[z].box))
		{
			zones[z]._players[id] = players[id];
			updateZoneOwner(zones[z].id);
			return;
		} 
		else 
		{
			if (zones[z]._players[id] != undefined && zones[z]._players[id] != null) 
			{
				delete zones[z]._players[id];
				updateZoneOwner(zones[z].id);
				
				return;
			}
		}
	}
}
function removeFromZones(id) {
	for (var z in zones)
	{
		if (zones[z]._players[id] != undefined && zones[z]._players[id] != null)
			{
				delete zones[z]._players[id];
				updateZoneOwner(z);
			}
	}
	
}
function updateZoneOwner(id) {
	if (count(zones[id]._players) == 1)
	{
		var _p = getRandomPlayerFromZone(zones[id]._players);
		
		if (zones[id].ownerID != zones[id]._players[_p].charId)
		{
			zones[id].setOwner(zones[id]._players[_p].charId)
			
			var zone = new ZoneData(id, zones[id].ownerID);
			
			clients[_p]._socket.emit('zone_update', zone);
			clients[_p]._socket.broadcast.emit('zone_update', zone);
			
			if (zones[id]._players[_p].charId === 0) {
				clients[_p]._socket.emit('chat', { sender: 'Notice', msg: `<color=#002bff>Knights</color> Captured a Camp! <color=#ffff00ff>+10 BP / 30 sec</color>`});			
				clients[_p]._socket.broadcast.emit('chat', { sender: 'Notice', msg: `<color=#002bff>Knights</color> Captured a Camp! <color=#ffff00ff>+10 BP / 30 sec</color>`});			
			} else {
				clients[_p]._socket.emit('chat', { sender: 'Notice', msg: `<color=#05cc05>Orcs</color> Captured a Camp! <color=#ffff00ff>+10 BP / 30 sec</color>`});			
				clients[_p]._socket.broadcast.emit('chat', { sender: 'Notice', msg: `<color=#05cc05>Orcs</color> Captured a Camp! <color=#ffff00ff>+10 BP / 30 sec</color>`});				
			}
		}
	} 
	else if (count(zones[id]._players > 1))
	{
	    var _p = getRandomPlayerFromZone(zones[id]._players);
	
		var allSame = true;
		var randomPlayerID = zones[id]._players.find(x=>x!=undefined).charId
		
		for (var p in zones[id]._players) 
		{
			if (p.charId != randomPlayerID)
			{
				allSame = false;
				return;
			}
		}
		
		if (allSame)
		{
			if (zones[id].ownerID != randomPlayerID) 
			{
				zones[id].setOwner(randomPlayerID);
				
				var zone = new ZoneData(id, zones[id].randomPlayerID);
				
				clients[_p]._socket.emit('zone_update', zone);
				clients[_p]._socket.broadcast.emit('zone_update', zone);
			}
		}
	}
}

io.on('connection', function (socket) {
	if (currentPlayers >= maxPlayers)
	{
		socket.emit('disconnect');
		return;
	}
	
	currentPlayers++;
	
	var isDead = false;
    var _id = shortId.generate();
    var client = {
        id:_id,
        name:"",
		isBot:false,
		_socket:socket,
    };
    clients[_id] = client;

    socket.emit('register', {id:_id});

	console.log("New client connected");
	
	for(z in zones) {
		var _z = new InitialZoneData(zones[z].id, zones[z].box.centerX, zones[z].box.centerY, zones[z].box.centerZ, zones[z].ownerID);
		socket.emit('zone_data', _z);
	}
	
	for(var playerId in players) {
		socket.emit('spawn', players[playerId]);
	}
	
    socket.on('name', function (data) {
        clients[_id].name = data.name;
	
		socket.emit('name', {id:_id});
		socket.emit('chat', { sender: 'Notice', msg: `<color=#ffff00ff>${data.name}</color> spawned in knor!`});			
		socket.broadcast.emit('chat', { sender: 'Notice', msg: `<color=#ffff00ff>${data.name}</color> spawned in knor!`});	
    });
    
    socket.on('spawn', function (data) {
		var _charId = data.charId;
		
		var pos = getSpawn(_charId);
		
		var _posX = pos.getX();
		var _posY = pos.getY();
		var _posZ = pos.getZ();
		
        var player = new Player(
            _id,
            clients[_id].name,
			clients[_id].isBot,
            _posX,
			_posY,
            _posZ,
            0,
            0,
            0,
            0,
			_maxHealth,
			_maxHealth,
			_charId,
        );
        players[_id] = player;	
		
		console.log("spawning player with id: " + _id);
		
        socket.broadcast.emit('spawn', players[_id]);
		socket.emit('spawn', players[_id]);
		
		socket.emit('score', {k1:warriorScore, k2:orcScore});
    });
	
    socket.on('move', function (data) {
		if (players[_id] != undefined && players[_id] != null)
		{
			if (!isDead)
				zoneCheck(data, _id);
			
			data.id = _id;
			players[_id].posX = data.x;
			players[_id].posY = data.y;
			players[_id].posZ = data.z;
			
			players[_id].rotX = data.rx;
			players[_id].rotY = data.ry;
			players[_id].rotZ = data.rz;
			players[_id].rotW = data.rw;
			
            socket.broadcast.emit('move', data);
        }
		else
			console.log("player with id: " + data.id + " not found!");
    });
	
	socket.on('jump', function (data) {
		socket.broadcast.emit('jump', data);
	});
	
	socket.on('damage', function (data) {
		if (players[data.id] != undefined && players[data.id] != null)
		{
			players[data.id].currentHealth -= data.damage;
			var died = false;
			
			var p = players[data.id];
			if (p.currentHealth <= 0) 
			{
				players[data.id].currentHealth = 0;
				died = true;
			}
			
			data.damage = players[data.id].currentHealth;
			socket.emit('damage', data);
			socket.broadcast.emit('damage', data);
			
			if (died)
			{
				socket.emit('died', {id:data.id});
				socket.broadcast.emit('died', {id:data.id});
						
				removeFromZones(data.id);
				
				if (p.charId == 0) {
					orcScore += killPoints;
					
					socket.emit('chat', { sender: 'Notice', msg: `<color=#ff0000ff>${players[data.id].name}</color> was defeated! <color=#ffff00ff>+1 BP</color>`});			
					socket.broadcast.emit('chat', { sender: 'Notice', msg: `<color=#ff0000ff>${players[data.id].name}</color> was defeated! <color=#ffff00ff>+1 BP</color>`});						
				}
				else {
					warriorScore += killPoints;
					
					socket.emit('chat', { sender: 'Notice', msg: `<color=#ff0000ff>${players[data.id].name}</color> was defeated! <color=#ffff00ff>+1 BP</color>`});			
					socket.broadcast.emit('chat', { sender: 'Notice', msg: `<color=#ff0000ff>${players[data.id].name}</color> was defeated! <color=#ffff00ff>+1 BP</color>`});				
				}
				
				isDead = true;
				
				socket.emit('score', {k1:warriorScore, k2:orcScore});
				socket.broadcast.emit('score', {k1:warriorScore, k2:orcScore});
		
			    clients[data.id]._socket.emit('respawn1', {id:data.id});
			}
		}
	});
	
	socket.on('respawn1', function (data) {
		if (data.accept)
		{
			isDead = false;
	           players[data.id].currentHealth = _maxHealth;
				
		        
		var pos = getSpawn(players[data.id].charId);
		
		var _posX = pos.getX();
		var _posY = pos.getY();
		var _posZ = pos.getZ();
				var respawnPacket = {
					id:data.id,
					name:players[data.id].name,
					isBot:clients[data.id].isBot,
					charId:players[data.id].charId,
					posX:_posX,
					posY:_posY,
					posZ:_posZ,
					maxhealth:_maxHealth,
					currentHealth:_maxHealth,
				};
		
				console.log("Respawning player with name: " + players[data.id].name + " at: " + _posX + " | " + _posY + " | " + _posZ);
				socket.emit('respawn2', respawnPacket);
				socket.broadcast.emit('respawn2', respawnPacket);
		}
		else 
		{
			socket.broadcast.emit('disconnected', { id:_id });
			socket.emit('disconnected', { id:_id });
			
		if (players[_id] != undefined && players[_id] != null)
		{
			removeFromZones(_id);
			delete players[_id];
		}
		
		if (client[_id] != undefined && client[_id] != null)
			delete clients[_id];
		
		currentPlayers--;
		
		data.id = _id;
		}
	});
	
	socket.on('attack_animation', function (data) {
		socket.broadcast.emit('attack_animation', data);
	});
	
	socket.on('reset_animation', function (data) {
		socket.broadcast.emit('reset_animation', data);
	});
	
	socket.on('chat', function (data) {
		socket.emit('chat', data);
		socket.broadcast.emit('chat', data);
	});
	
	socket.on('bot_registration', function (data) {
		clients[data.id].isBot = true;
	});
	
    socket.on('ai_anims', function (data) {
		socket.emit('ai_anims', data);
		socket.broadcast.emit('ai_anims', data);
    });
	
    socket.on('ai_anim', function (data) {
		socket.emit('ai_anim', data);
		socket.broadcast.emit('ai_anim', data);
    });
	
    socket.on('ai_animation', function (data) {
		socket.emit('ai_animation', data);
		socket.broadcast.emit('ai_animation', data);
    });

    socket.on('disconnect', function (data) {
		console.log("A client has disconnected!");
		console.log("disconnected", data);
		if (players[_id] != undefined && players[_id] != null)
		{
			socket.emit('chat', { sender: 'Notice', msg: `<color=#c0c0c0ff>${players[_id].name}</color> left the battle...`});			
			socket.broadcast.emit('chat', { sender: 'Notice', msg: `<color=#c0c0c0ff>${players[_id].name}</color> left the battle...`});				
			removeFromZones(_id);
			delete players[_id];
		}
		
		if (client[_id] != undefined && client[_id] != null)
			delete clients[_id];
		
		currentPlayers--;
		
		data.id = _id;
		
		socket.broadcast.emit("disconnected", { id : _id });	
    });
});