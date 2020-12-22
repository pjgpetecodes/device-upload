# .NET Azure IoT Hubs Device File Upload Demo

A .NET Core Azure IoT Hubs Device File Upload Demo App

# Instructions

- Create a new Azure IoT Hub in the Standard or Free Tier.
- Create an IoT Device.
- Copy the Primary Connection String from the IoT Device
- Go to `File Upload` in the IoT Hub
- Click `Azure Storage Container` under `Storage container settings`
- Either create a new Storage Account by pressing `+ Storage account` or select an existing Storage Account
- Either create a new Container by pressing the `+ Container` button or select an existing container
- Press the `Select` button.
- Press the `Save` button

- Run the Demo application with;

```
dotnet run
```

- Paste the IoT Device Primary Connection String into the console when prompted
- The `test.csv` File will be uploaded
- Navigate to the Storage Account
- Navigate to `Storage Explorer (preview)`
- Expand Blob Containers
- Click on your Container
- Double Click on the folder named the same as your IoT Device
- The Uploaded File should be shown.