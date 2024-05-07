# Indroduction

Welcome to the UserProfileService (abbreviated: **UPS**) documentation. It should provide you with all necessary information to work with this powerful service.

## What is the UserProfileService?
The User Profile Service (short: UPS) is a system designed to manage user data and related entities such as groups, organizations, roles, or functions. 
It consists of three main components:


1. **Service**: The UPS provides a comprehensive API that allows users to modify entities
In this context, "modify" refers to performing CRUD (Create, Read, Update, Delete) operations. This API likely includes endpoints for actions such as creating, updating, deleting, and querying user profiles, groups, organizations, roles, and functions.

1. **Saga Worker**: This component is responsible for validating and creating or modifying entities based on the requests received through the API. It likely handles business logic, data validation, and ensures consistency and integrity of the data. The Saga Worker can be considered the centerpiece of the UPS.

1. **Sync Service**: The Sync Service is used to synchronize entities from a third-party system, specifically LDAP (Lightweight Directory Access Protocol) systems in this case. This component facilitates the synchronization of user data and related entities between the UPS and LDAP systems.

The UPS provides a wide range of functionality that will be explained further in this documenation.

## History / Motivation

### BMBF-Project

> "Das Bundesministerium für Bildung und Forschung (BMBF) unterstützt die Durchführung von Forschungs- und Innovationsprojekten im Rahmen von themenspezifischen und in themenoffenen Förderprogrammen. Das breite Förderangebot ist auf wichtige Innovations- oder Technologiefelder, aber auch auf unterschiedliche Herausforderungen und Ausgangsbedingungen zugeschnitten. Dabei werden insbesondere innovative kleine und mittlere Unternehmen (KMU) mit spezifischen Förderprogrammen adressiert." ([Quelle - Bundesministerium für Bildung und Forschung](https://www.bmbf.de/bmbf/de/forschung/zukunftsstrategie/foerderung-in-der-forschung/foerderung-in-der-forschung_node.html))

**Translation**: The Federal Ministry of Education and Research (BMBF) supports the implementation of research and innovation projects within topic-specific and open-topic funding programs. The broad range of funding is tailored to important innovation or technology fields, as well as various challenges and starting conditions. In particular, innovative small and medium-sized enterprises (SMEs) are addressed with specific funding programs.


> "Im Zuge der Förderung entstand das Projekt E365 Maverick. Es ist ein durch das BMBF gefördertes Forschungsprojekt, welches im Rahmen einer Kooperation zwischen Bechtle AG und Hochschule Bonn-Rhein-Sieg durchgeführt wird. Es bettet sich in die übergeordnete Initiative „Nationale Bildungsplattform“ des BMBF ein." ([Quelle - E365 Maverick Projekt](https://www.h-brs.de/de/eagl-digitale-bildungsplattform)) 

**Translation**: As part of the funding, the project E365 Maverick was created. It is a research project funded by the BMBF, which is being conducted in cooperation between [Bechtle AG](https://www.bechtle.com/) and the Bonn-Rhein-Sieg University of Applied Sciences. It is embedded within the overarching initiative 'National Education Platform' of the BMBF.


### Splitting the UPS
Bechtle provides the UPS, which is intended for user data within the digital education platform. Since GitHub is the largest host of open-source projects, the UPS is made available on this platform. Furthermore, the community within the host is very active, leading to an increase in software quality. Improvements can be noted and discussed by the community, largely ensuring a continuous development of the software with the latest technologies and design patterns.

As the current code base of the UPS is currently in an internal project within Azure DevOps Services, this will continue to be used for the customer-specific part.