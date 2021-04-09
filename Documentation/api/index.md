# API Documentation

## Namespaces

### UnderDeskBike

This namespace contains the main [Bike](xref:UnderDeskBike.Bike) interface class and related classes.

| Class                                                             | Description                   |
|-------------------------------------------------------------------|-------------------------------|
| [Bike](xref:UnderDeskBike.Bike)                                   | The main class for recording workouts from the bike.      |
| [BikeDataException](xref:UnderDeskBike.BikeDataException)         | Raised when an error occurs receiving data from the bike. |
| [BikeWorkoutData](xref:UnderDeskBike.BikeWorkoutData)             | A sample of workout data from the bike. |
| [BikeWorkoutEventArgs](xref:UnderDeskBike.BikeWorkoutEventArgs)   | Complex tasks which require Vector's internal logic to complete |

### UnderDeskBike.Models

This namespace contains the classes for interfacing with the SQLite database containing the workout data and samples.

| Class                                                             | Description                   |
|-------------------------------------------------------------------|-------------------------------|
| [Context](xref:UnderDeskBike.Models.Context)                      | The database context for the bike data.   |
| [Workout](xref:UnderDeskBike.Models.Workout)                      | Summary data for a workout.               |
| [WorkoutEntry](xref:UnderDeskBike.Models.WorkoutEntry)            | A single sample from the bike.            |



### Codaris.Common

A home-grown library of helper classes.
