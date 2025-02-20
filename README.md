# Moneybox Money Withdrawal

## Description 
My approach involved shifting some of the account domain-related logic into the Account object. The Account object now restricts external calls to directly update the Balance, Withdrawn, and PaidIn values, as these updates should be handled internally by the object's logic. Additionally, PayInLimit and NotificationThreshold have been made private to prevent external modifications, ensuring these constants remain consistent across all business areas.

Unit tests have been implemented to verify the correctness of the Account object's logic. These tests also ensure that the TransferMoney and WithdrawMoney operations perform as expected, updating Balance, Withdrawn, and PaidIn values accurately.

## Future Improvements
1. Create e2e project to verify classlib will behave correctly, this will be done by creating interface for both TransferMoney, Withdraw
2. Add Service collection extension methods so that these TransferMoney, Withdraw Money class can be regirestred in IOC easily.
3. MS logging will be added to this classlib in order to produce logs for unexpected events.
4. Metrics will be added to make sure visibility is present for when TransferMoney and WithdrawMoney are executed.

## Packages Used
1. Shoudly
2. Moq
3. Autofixture

## Build 
To build this project please run "dotnet clean && dotnet restore && dotnet build"

## Test
This project has a test project which uses Xunit, the folder structure is the same as the MoneyBox.App with tests for class's which have been refactored or edited
Please run "dotnet test"

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* The test should take about an hour to complete, although there is no strict time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your work
* Your solution must build and any tests must pass
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Once you have completed test, zip up your solution, excluding any build artifacts to reduce the size, and email it back to our recruitment team.

Good luck!
