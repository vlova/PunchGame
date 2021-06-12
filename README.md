# PunchGame
 
Test project for gamedev company

# Overall architecture

Components:
1. Client without predictions
2. Single process server with multiple rooms

Highlights:
- Communication via TCP+JSON

# Game (room) architecture

Components:
- Mutable state
- Commands & command handlers
- Events & event reducers

Flow:
1. Received tcp event is transformed into one of GameCommands
2. Game commands are grouped by time quant. By this we ensure funny behavior like "two players can kill each other". But this is not required
3. For each quant we do next:
   1. Execute command handlers, which are producing events only (doesn't mutate state)
   2. Execute event reducers, which are mutating state only (doesn't produce more events yet)
4. Transmit events:
   - Broadcast events to everyone who plays in room
   - Personal events to specific player
   - Internal events to scheduler subsystem