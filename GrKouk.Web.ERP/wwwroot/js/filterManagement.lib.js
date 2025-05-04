//Author: George Koukoudis
//Version:  1.0.0
//Date Created: 2025-05-4
//Date Modified: 2025-05-4
//Filter Management javascript tools

const fltManLib = (function () {
    const saveSettingsToStorage=(storageKey, config)=>{
        const settingsToStore = [];

        // Iterate through config and read settings using jQuery
        for (const elementId in config) {
            if (Object.hasOwnProperty.call(config, elementId)) {
                //const { key: settingKey, prop } = config[elementId];
                const { key: settingKey, prop, dataType } = config[elementId]; // Get dataType

                const $element = $('#' + elementId); // Use jQuery selector

                if ($element.length) { // Check if element exists
                    try {
                        let currentValue;
                        if (dataType === 'array') {
                            currentValue = $element.val();
                        }
                        else if (prop === 'checked') {
                            // Use .prop() to read boolean properties
                            currentValue = $element.prop('checked');
                        } else if (prop === 'value') {
                            // Use .val() to read 'value'
                            currentValue = $element.val();
                        } else {
                            // Fallback for potentially other properties
                            currentValue = $element.prop(prop);
                            console.warn(`Attempting to read non-standard property '${prop}' using .prop() from '#${elementId}'. Verify this is intended.`);
                        }
                        settingsToStore.push({ filterKey: settingKey, filterValue: currentValue });
                    } catch (e) {
                        console.error(`Error reading property/value from jQuery element '#${elementId}'. Skipping this setting.`, e);
                    }
                } else {
                    console.warn(`Element with ID "${elementId}" not found in the DOM using jQuery during save. Skipping this setting.`);
                }
            }
        }

        // Save the array to localStorage (no change here)
        try {
            const jsonString = JSON.stringify(settingsToStore);
            localStorage.setItem(storageKey, jsonString);
            console.log(`Settings saved successfully to localStorage storageKey "${storageKey}".`);
        } catch (error) {
            console.error(`Failed to save settings to localStorage key "${storageKey}".`, error);
        }


    }
    const applySettingsFromStorage=(storageKey, config)=>{
        const storedString = localStorage.getItem(storageKey);
        let loadedSettings = {};

        if (storedString) {
            try {
                const storedArray = JSON.parse(storedString);
                if (Array.isArray(storedArray)) {
                    loadedSettings = storedArray.reduce((acc, item) => {
                        if (item && typeof item.filterKey === 'string') {
                            acc[item.filterKey] = item.filterValue;
                        }
                        return acc;
                    }, {});
                } else {
                    console.warn(`localStorage item "${storageKey}" is not a valid array. Using defaults.`);
                }
            } catch (error) {
                console.error(`Failed to parse localStorage item "${storageKey}". Using defaults.`, error);
            }
        } else {
            console.log(`localStorage item "${storageKey}" not found. Using defaults.`);
        }

        // Iterate through config and apply settings using jQuery
        for (const elementId in config) {
            if (Object.hasOwnProperty.call(config, elementId)) {
                //const { key: settingKey, default: defaultValue, prop } = config[elementId];
                const { key: settingKey, default: defaultValue, prop, dataType } = config[elementId];

                const $element = $('#' + elementId); // Use jQuery selector

                if ($element.length) { // Check if element exists using jQuery's length property
                    let value = loadedSettings[settingKey] !== undefined ? loadedSettings[settingKey] : defaultValue;
                    try {
                        // --- Special Handling for Array ---
                        if (dataType === 'array') {
                            $element.val(value); // Set using .val()
                        }

                        else if (prop === 'checked') {
                            // Use .prop() for boolean properties like 'checked'
                            $element.prop('checked', Boolean(value));
                        } else if (prop === 'value') {
                            // Use .val() for 'value' property
                            $element.val(value);
                        } else {
                            // Fallback for potentially other properties (less common with jQuery)
                            $element.prop(prop, value);
                            console.warn(`Attempting to set non-standard property '${prop}' using .prop() on '#${elementId}'. Verify this is intended.`);
                        }
                    } catch (e) {
                        console.error(`Error setting property/value on jQuery element '#${elementId}' with value '${value}'.`, e);
                    }
                } else {
                    // console.warn(`Element with ID "${elementId}" not found in the DOM using jQuery during apply.`);
                }
            }
        }

    }
    return {
        saveSettingsToStorage: saveSettingsToStorage,
        applySettingsFromStorage: applySettingsFromStorage
    };
})();