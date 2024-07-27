# Task Management Application

Web application implemented using `C#` and `ASP.NET Core MVC` framework that allows users to create projects, teams, tasks, and comments. 

<br /><br />
![Screenshot (828)](https://user-images.githubusercontent.com/93484228/221356800-a0a31236-524a-4801-b890-bb964cd9f06f.png)
<br /><br />

### Full Description <br />

&emsp; This app uses an Identity Authentification System, that accepts users and roles. Here, the roles used are Admin and User. Therefore there is a rule that says that when a user creates a project in this app he becomes the Organizer of that project, meaning that he can create a team that can have assigned that project, and in that team, he can add or remove members and tasks. Note that the teams a user can create must have a project that he is the organizer of and that it's not already assigned to another team. If a user does not have a project, but he wants to create a team, he will not be allowed to do that, because he must be the organizer of the project that will be assigned to the team first. If the user it's just a member of the team, he can not perform those operations, and the only thing he can do it's to edit the tasks that are assigned to him or leave comments on the tasks that belong to the team that he is in. <br /><br />

&emsp; To make this app easy and efficient to use, I have applied a new rule so that the connected user can only see the projects that belong to him (where he is an organizer) and the projects that he is a member of. With this new rule, I made sure that the registered user will not receive a long list that contains thousands of projects that he can not access, edit or perform any operation onto. <br /><br />

&emsp; The admin can add, edit or remove teams, projects, tasks, comments, members in the teams, and even users. He can change any project organizer and I called that operation 'revoking the rights' of a user. <br /><br />

### Rules of the app <br />
&emsp;&emsp;-> the unregistered users can only view the Home page of the platform and the authentification and registration forms <br />
&emsp;&emsp;-> the user that creates a project become organizer of that project <br />
&emsp;&emsp;-> the organizer can create a team with one of the projects that he created assigned, add members to that new team, and create, edit, and delete tasks for that team <br />
&emsp;&emsp;-> team members can only leave comments on the existent tasks and edit or delete their comments <br />
&emsp;&emsp;-> team members can only view the tasks and comments from a team that they are a member of <br />
&emsp;&emsp;-> both the organizer and the team members of a specific team can change the status of a task (not assigned, not started, in progress, completed) <br />
&emsp;&emsp;-> the admin provides the good functionality of the platform and can perform any kind of operation on any entity <br />

### Some insights 
![Screenshot (832)](https://user-images.githubusercontent.com/93484228/221356944-2dfe4b27-531c-42cc-b874-8b62ac0274dc.png)
<br />
&emsp;&emsp;&emsp;Here it can be seen that I have logged in as Admin and I am currently on the page of a team I can see all the members of that team, the tasks, and the description of the project assigned to it. It is also clear, that I can perform a lot of operations on those entities
<br /><br />
![Screenshot (833)](https://user-images.githubusercontent.com/93484228/221357138-b069da20-f1a6-4915-b4ae-361cffbb88be.png)
<br />
&emsp;&emsp;&emsp;In this print-screen I am the Admin again and I am currently looking at all the comments in the app it's quite clear that I can perform any operation I want on all the comments even if I am not the one that posted them
<br /><br />
![Screenshot (829)](https://user-images.githubusercontent.com/93484228/221357249-83247cd8-abe6-4716-ac95-bf4255b1bd27.png)
![Screenshot (835)](https://user-images.githubusercontent.com/93484228/221357262-14fada88-43c9-4596-85d8-5f08665274aa.png)
<br />
&emsp;&emsp;&emsp;Those pictures provide a clear distinction between the roles used in this app, and how a user that has not to have many projects will not see a long list of projects, because he can not perform any operations on them and it's useless to see all of them. This rule also applies to the teams, comments, and tasks, because we want to provide efficient navigation for all the users in this app


