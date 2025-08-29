import { useState, useEffect } from 'react';
import { message } from 'antd';
import { Company, fetchCompanies } from '../features/companies/companyAPI';
import { Unit, fetchUnits } from '../features/units/unitsAPI';
import { Alarm, fetchAlarms } from '../features/alarms/alarmAPI';
import { Trigger, fetchTriggers } from '../features/triggers/triggerAPI';
import { UnitModel, fetchUnitModels } from '../features/unitmodels/unitModelAPI';

export const useVisualizationData = (accessToken: string | null) => {
    const [companies, setCompanies] = useState<Company[]>([]);
    const [units, setUnits] = useState<Unit[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [triggers, setTriggers] = useState<Trigger[]>([]);
    const [unitModels, setUnitModels] = useState<UnitModel[]>([]);
    const [loading, setLoading] = useState(false);

    const loadData = async () => {
        if (!accessToken) return;

        setLoading(true);
        try {
            const [companiesData, unitsData, alarmsData, triggersData, unitModelsData] = await Promise.all([
                fetchCompanies(accessToken),
                fetchUnits(accessToken),
                fetchAlarms(accessToken),
                fetchTriggers(accessToken),
                fetchUnitModels(accessToken)
            ]);

            setCompanies(companiesData);
            setUnits(unitsData);
            setAlarms(alarmsData);
            setTriggers(triggersData);
            setUnitModels(unitModelsData);

            return { companiesData, unitsData, alarmsData, triggersData, unitModelsData };
        } catch (error) {
            console.error('Failed to load visualization data:', error);
            message.error('Failed to load data');
            throw error;
        } finally {
            setLoading(false);
        }
    };

    return {
        companies,
        units,
        alarms,
        triggers,
        unitModels,
        loading,
        loadData
    };
};