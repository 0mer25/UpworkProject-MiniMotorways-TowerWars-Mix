Generally codes handles everything after level creation part



--- How To Create A New Level --- 

1- Duplicate the 'BaseLevelTemplate' prefab

2- Place the buildings anywhere you want (ofcourse they must placed on 3x3 matrixes perfectly)

3- Close the buildings which you want to spawn later

4- Add all the buildings to the 'BaseLevel' component's (Which is the duplicated level object) 'AllBuildings' list

5- Enter the 'Total Road Tiles' and 'LevelIndex' to the 'BaseLevel' component

6- Place the multiplier gates and obstacles anywhere you want

7- Delete the 'TilesWillDelete' object in the prefab

8- Lastly, Add the new level prefab to the 'Levels' list which is in the 'LevelManager' component which is in the 'LevelManager' object which is in the Game Scene.



------

For the functions there is a few functions that you need to know and all of them are understandable by their names easily.


For adding more teams developers need to add the new team to the 'Team' script a new enum with the new team name. Then you need to duplicate the prefabs for that team also.


There are scriptables for building and car stats. Developers can easily change everything.