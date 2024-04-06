# NugECS  
A learning project for a E-CS style framework.  
Plan to make both "real" ECS and E-CS work. See TestApp for example.  
While ECS is viable in it (aka using queries) it is not the main focus. Think Unity style ECS but transforms aren't components but something every entity has.

### Test App performance (1280x720):  
__E-CS variable update__: 60fps 150000  
__E-CS fixed update__: 60fps 146000  
__ECS low alloc queries__: 60fps 69000  
__ECS high allow queries__: 60fps 81000 

### Rig:  
__Cpu__: i7 4790k  
__Ram__: 32gb ddr3 1600mhz  
__GPU__: rtx3070  
