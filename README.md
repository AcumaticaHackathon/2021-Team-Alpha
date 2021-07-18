# 2021-Team-Alpha 

## AcuChecker

Scenario:

A developer adds a custom field to a DAC within a Visual Studio extension library, and then creates the corresponding SQL table column in their local database. At this point, they forget to update the Database Script in the customization project to register the new field to the SQL table schema.  

In this scenario, the customization successfully publishes in the local environment with no errors. However, when the developer exports the customization project from their local Acumatica development instance and imports the package into a test instance, the newly-added DAC fields are not added to the test database.  The customization project publishes successfully without any errors, but the new custom DAC fields do not have a corresponding column in the SQL table - and now the problem is not apparent until a screen attempts to select data from the SQL table database and an "Invalid column name" error interrupts any screen that uses the DAC.

During publish of packages AcuChecker will test all DLL's inside the Customization Projects published.
The output of the tests will be sent to the compllation screen.

![alt text](https://github.com/AcumaticaHackathon/2021-Team-Alpha/blob/Library/Capture.PNG?raw=true)
