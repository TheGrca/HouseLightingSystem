# HouseLightingSystem
This SCADA system involves the design and implementation of an advanced lighting control system for a building, leveraging a central control unit to manage various types of lighting (interior, exterior, and security) through different channels. The system comprises three interior lights (L1, L2, L3), two exterior lights (L4, L5), and one security light (L6) that must be activated in emergency situations. The energy consumption rates for these lights are specified: interior lights consume 5% of battery energy every 10 minutes, exterior lights consume 10%, and the security light consumes 3%.

Additionally, the system features two power inputs for battery charging: Input I1 charges the battery at 5% per minute, while Input I2 charges at 8% per minute. A configuration file, RtuCfg.txt, must be extended to include new parameters such as a scaling factor (A), offset (B), lower alarm threshold (LowAlarm), maximum battery capacity (EguMax), and abnormal state indicators (AbnormalValue). The default values for these parameters are set to ensure smooth operation and fault tolerance of the system.

The configuration stipulates the following operational and control requirements:

- Periodic reading and updating of all digital outputs (coils) on the user interface.
- Periodic reading and updating of all analog outputs (holding registers) on the user interface.
- Enabling command control through the control window for all defined digital outputs (coils) and refreshing the values on the user interface upon successful write operations.
- Enabling command control through the control window for all defined analog outputs (holding registers) and refreshing the values on the user interface upon successful write operations.
- Converting engineering units back to raw values when issuing commands to analog outputs, utilizing the parameters A and B.
- Allowing users to manually (remotely) turn on lights (L1-L6) only if the battery capacity exceeds the LowAlarm threshold.
- Triggering a LowAlarm and turning off the exterior lights (L4 and L5) to conserve energy when the battery capacity (K) drops below the LowAlarm value, and activating Input I2 for faster battery charging.
- Stopping the charging input (I1 or I2) once the battery capacity reaches the EguMax value.
- Simulating battery charging and discharging based on the status of the lights and power inputs.

This project requires a comprehensive understanding of digital and analog control systems, energy management, and user interface design. The goal is to ensure efficient energy usage, robust fault handling, and intuitive control over the building's lighting system.
