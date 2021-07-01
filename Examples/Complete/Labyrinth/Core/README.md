# MAZE
Welcome to the Maze Demo FUSEE app, it's there to test collision detection in 2D space.

`Maze.cs` contains the source code for the FUSEE application.
The models are in  `.bend` in the `Art folder` and in `.fus` in `Assets folder`.

## Control
* Camera
    * Press and hold the left mouse button and move the mouse to look around from the position of the robot.
   
    * With the right mouse button, you can also go into first person mode and back.
   
    * If you press E you can get a view of the Maze, but you cannot move in this mode


* Movement:
    * The movement is carried out with the WASD controls. The movement rotates depending on where you look.

## Structure of the Maze
Since there are only two walls, a cornerstone and the robot in the `.fus`, the maze is created using a `2D-Array` that was created in front of it.

## Collision detection
This works with the help of the `bounding box` and the `2D-Array`. First, the `2D-Array` is used to check the exact position of the robot. If the robot continues to move, the position is also changed in the `2D-Array`.

![2D-Array Movement](art/2D-Array.png "2D-Array Movement")

If it is clear where the body is, it tests in front, next, and behind it if there are walls. If there is a wall in the direction, it is tested whether there is a collision. This is done by querying whether the x position and the center of the body of the robot is smaller than the radius of it. If this is the case, he should not drive any further in the direction. It is important to have the bounding boxes of the walls, since the bounding box contains the `minimum x and z positions` from the wall and the `maximum x and z positions`.

Like the walls, the corners are only recognized this time in the slants, i.e. front- right/left of him, or back- right/left of him. Then a circle and a rectangle are used to query whether they overlap.
The collision detection for this is like the Pythagorean theorem:

![Pythagorean theorem](art/Pythagorean-theorem.png "Pythagorean theorem")

The goal is viewed like a wall only that if you hit it, the goal condition will be met.

## Rotation 
The robot consists of three parts: the `head`, a `neck` and the `body`.

![structure](art/structure.png "structure")

* Head 
    * rotates around the `Y-Axis` when the camera is moving and in the direction of the `XZ-Axes` depending on the movement. It should also be mentioned that the head has its center in the center of the body, which is why it rotates around the body and not around itself.

* Neck 
    * is an empty node that is there to compensate for the Y rotation of the head. Therefore, it turns in the opposite y direction to the head so that the body stays in the same place.

* Body 
    * rotates with the help of quaternions depending on the direction of the movement.

* Why use quaternions?
I decided to use them because they bypass the `"gimbal lock"`. There is also the method that they rotate around any given axis, which is easy because you only have to specify the vector. If you want to know more about `quaternions` I can recommend the YouTube channel of `3Blue1Brown` which explains them relatively well and understandably.


## Additive
In addition to the main program, there is also an additional file that contains the reading of a maze template. The template is a bitmap with which the program creates the associated `2D-Array`.

In order to create your own Maze you have to consider the following in the template file. You can find it in the `art folder`. The `walls / cornerstones` are the thin rectangles and the small squares. You can fill them with `black color` to place them. To get an `entrance` or `exit` you have to fill the walls with `red color`. For the `starting point` you have to color one of the large squares `green`.

IMPORTANT: The colors should always be maximum  
Red = (255,0,0)  
Green = (0,255,0)  
Black = (0,0,0) 

Once you've done all this, you can save your Maze in the `assets folder` under the name `Maze.bmp`



