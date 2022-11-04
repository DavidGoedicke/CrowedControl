## ToDOs:
Now:
- [ ] Interaction placing signs.  
- [ ] Interaction placing walls.




Later:
 - [ ] How do we properly keep track of a walking direction. Espeically if we are missing an influencing vector. If no other unit is arround we should not ust set it to zero. If the motion vector is already zero we should go just forward, this should be done in the `AgentSystem.cs` and no `ApplAgentMotion.cs`

 

 ## Ideas:
  - [ ] fan out the gate and sign finding ray cast.( to a max value) based on the time the agent has not seen a sign. 