import "bootstrap/dist/css/bootstrap-reboot.min.css";
import "bootstrap/dist/css/bootstrap-utilities.min.css";
import { Route, Routes } from "react-router";
import { DefaultLayout } from "./layout/DefaultLayout";
import { HomePage } from "./pages/HomePage";
import { RegisterPage } from "./pages/RegisterPage";
/*import { LoginPage } from "./pages/LoginPage";*/
import { NotFoundPage } from "./pages/NotFoundPage";
import { useAppSelector } from "./app/hooks";
import { selectAuth } from "./app/store";
import { UserLayout } from "./layout/UserLayout";
import { Spin } from "antd";
import UsersList from "./pages/UsersList";
import EditProfile from "./pages/EditProfile";
import UnitList from "./pages/UnitList";
import SystemStatus from "./pages/SystemStatus";
import { CompanyList } from "./pages/CompanyList";

export const App = () => {
    const auth = useAppSelector(selectAuth);
    if (!auth.user) {
        return (
            <Spin spinning={auth.status == "loading"}>
                <Routes>
                    <Route path="/" element={<DefaultLayout />}>
                        <Route index element={<SystemStatus />} />
                        <Route path="register" element={<RegisterPage />} />
                        {/*<Route path="login" element={<LoginPage />} />*/}
                        <Route path="*" element={<NotFoundPage />} />
                    </Route>
                </Routes>
            </Spin>
        );
    } else {
        return (
            <>
                <Spin spinning={auth.status == "loading"}>
                    <Routes>
                        <Route path="/" element={<UserLayout {...auth.user} />}>
                            <Route index element={<HomePage />} />
                            <Route path="/users" element={<UsersList />} />
                            <Route path="/units" element={<UnitList />} />
                            <Route path="/system-status" element={<SystemStatus />} />
                            <Route path="/edit-profile" element={<EditProfile />} />
                            <Route path="/companies" element={<CompanyList />} />
                            <Route path="*" element={<NotFoundPage />} />
                        </Route>
                    </Routes>
                </Spin>
            </>
        );
    }
};

export default App;