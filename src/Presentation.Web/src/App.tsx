import { useEffect, useState } from "react";
import "./App.scss";
import { Route, Routes, useParams } from "react-router-dom";
import { DefaultLayout } from "./layout/DefaultLayout";
import { RegisterPage } from "./pages/RegisterPage";
import { NotFoundPage } from "./pages/NotFoundPage";
import { useAppSelector } from "./app/hooks";
import { selectAuth } from "./app/store";
import { UserLayout } from "./layout/UserLayout";
import { Spin } from "antd";
import UsersList from "./pages/UsersList";
import EditProfile from "./pages/EditProfile";
import UnitList from "./pages/UnitList";
import AlarmList from "./pages/AlarmList";
import SystemStatus from "./pages/SystemStatus";
import AlarmInstructions from "./pages/AlarmInstructions";
import CompanyList from "./pages/CompanyList";
import SensorList from "./pages/SensorList";
import SensorManagement from "./pages/SensorManagement";
import ReadingsList from "./pages/ReadingsList";
import UnitModelList from "./pages/UnitModelList";
import Dashboard from "./pages/Dashboard";
import ConfigurationUploads from "./pages/ConfigurationUploads";
import RecipientList from "./pages/RecipientList";
import RecipientSetList from "./pages/RecipientSetList";
import TriggerList from "./pages/TriggerList";
import Visualization from "./pages/Visualisation";
import { LoginPage } from "./pages/LoginPage";

// Wrapper component for readings with sensor parameter
const ReadingsWithSensor = () => {
    const { sensorId } = useParams<{ sensorId: string }>();
    return <ReadingsList sensorId={sensorId ? parseInt(sensorId, 10) : undefined} />;
};

// Wrapper component for readings with unit parameter
const ReadingsWithUnit = () => {
    const { unitId } = useParams<{ unitId: string }>();
    return <ReadingsList unitId={unitId ? parseInt(unitId, 10) : undefined} />;
};

const App = () => {
    const auth = useAppSelector(selectAuth);
    const [isInitialized, setIsInitialized] = useState(false);
    
    // Handle initial app load - don't show spinner during startup
    useEffect(() => {
        // Give a brief moment for Redux Persist to rehydrate
        const timer = setTimeout(() => {
            setIsInitialized(true);
        }, 100);
        
        return () => clearTimeout(timer);
    }, []);
    
    // Only show loading spinner during actual authentication operations
    // Not during initial app load or when already authenticated
    const shouldShowLoadingSpinner: boolean = isInitialized && 
        auth.status === "loading" && 
        // Don't show spinner if we're just starting up
        !!(auth.accessToken || auth.refreshToken || auth.user);
    
    if (!auth.user) {
        return (
            <div className="app-container">
                {shouldShowLoadingSpinner ? (
                    <div>
                        <Spin size="large" tip="Loading...">
                            <div style={{ padding: '50px' }} />
                        </Spin>
                    </div>
                ) : (
                    <Routes>
                        <Route path="/" element={<DefaultLayout />}>
                            <Route index element={<LoginPage />} />
                            <Route path="login" element={<LoginPage />} />
                            <Route path="system-status" element={<SystemStatus />} />
                            <Route path="register" element={<RegisterPage />} />
                            <Route path="*" element={<NotFoundPage />} />
                        </Route>
                    </Routes>
                )}
            </div>
        );
    } else {
        return (
            <div className="app-container">
                {shouldShowLoadingSpinner ? (
                    <div>
                        <Spin size="large" tip="Loading...">
                            <div style={{ padding: '50px' }} />
                        </Spin>
                    </div>
                ) : (
                    <Routes>
                        <Route path="/" element={<UserLayout {...auth.user} />}>
                            <Route index element={<Dashboard />} />
                            <Route path="/dashboard" element={<Dashboard />} />
                            <Route path="/readings" element={<ReadingsList />} />
                            <Route path="/readings/sensor/:sensorId" element={<ReadingsWithSensor />} />
                            <Route path="/readings/unit/:unitId" element={<ReadingsWithUnit />} />
                            <Route path="/users" element={<UsersList />} />
                            <Route path="/units" element={<UnitList />} />
                            <Route path="/system-status" element={<SystemStatus />} />
                            <Route path="/edit-profile" element={<EditProfile />} />
                            <Route path="/companies" element={<CompanyList />} />
                            <Route path="/sensors" element={<SensorManagement />} />
                            <Route path="/unitmodels" element={<UnitModelList />} />
                            <Route path="/alarminstructions" element={<AlarmInstructions />} />
                            <Route path="/alarms" element={<AlarmList />} />
                            <Route path="/triggers" element={<TriggerList />} />
                            <Route path="/visualisation" element={<Visualization />} />
                            <Route path="/recipients" element={<RecipientList />} />
                            <Route path="/recipientsets" element={<RecipientSetList />} />
                            <Route path="/configurationuploads/:unitId" element={<ConfigurationUploads />} />
                            <Route path="*" element={<NotFoundPage />} />
                        </Route>
                    </Routes>
                )}
            </div>
        );
    }
};

export default App;