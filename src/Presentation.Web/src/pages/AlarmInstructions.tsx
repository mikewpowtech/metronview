import React from 'react';
import { InfoCircleOutlined } from '@ant-design/icons';

const AlarmInstructions: React.FC = () => {
    return (
        <div style={{ padding: "12px 0 12px 30px" }} >
            <h1>
                Alarms{' '}
            </h1>
            <p>
                Alarms send alerts to sets of recipients when particular conditions happen. They are managed the
                following stages.
            </p>

            <p className="bg-light p-2">
                <InfoCircleOutlined style={{ color: '#1890ff', fontSize: 22, verticalAlign: 'middle' }} title="Alarm instructions" /> Please contact your provider if you wish to receive alarms by SMS.
            </p>

            <h2 className="h3">Define recipients</h2>
            <p>
                Recipients are people or services who can receive alarms. Each recipient has a name, and perhaps an
                email address, SMS number, or Web service root if the recipient is a service. Recipients can also be
                enabled or disabled; you might choose to disable a recipient if they're going on holiday, for example.
            </p>
            <p>To manage recipients, choose Manage Recipients from the Alarms menu.</p>
            <h2 className="h3">Define recipient sets</h2>
            <p>
                Often, you want one alarm to send to more than one recipient. Perhaps you want to SMS one person and
                email another. Recipient sets group recipients together for alarms. You can have many recipient sets,
                and each recipient can be in as many sets as you like.
            </p>
            <p>To manage recipient sets, choose Manage Recipient Sets from the Alarms menu.</p>
            <h2 className="h3">Define alarm sets</h2>
            <p>
                Alarms are grouped into sets, and each set can be allocated to multiple sensors. For example, if you're
                managing fuel tanks, you might want an alarm set containing three alarms: a &quot;low&quot; alarm when
                the level falls below 40%, a &quot;very low&quot; alarm when the level falls below 20%, and a &quot;fuel
                theft&quot; alarm when the remote unit sends a rate-of-change alarm. You can create one alarm set that
                will group all three alarms, and then assign that across all your sensors that need 40%/20%/theft
                alarms.
            </p>
            <p>An alarm set also defines who is notified when any of the alarms in the set are triggered.</p>
            <p>To manage alarm sets, choose Manage Alarm Sets and Alarms from the Alarms menu.</p>

            <h2 className="h3">Define alarms</h2>
            <p>
                An alarm set can contain as many alarms as you wish. Each alarm states how it is triggered, and
                specifies a response. Alarms can also be enabled or disabled.
            </p>
            <p>To manage alarms, choose Manage Alarm Sets and Alarms from the Alarms menu.</p>

            <h2 className="h3">Assign alarm sets to sensors</h2>
            <p>
                Once the alarm sets are ready to go, you can associate any sensor with an alarm set. As soon as you do
                this, the system will start to monitor readings about the sensor sent from the remote unit, and will
                fire matching alarms.
            </p>
            <p>To assign alarm sets to sensors, choose and edit a sensor from a unit in Manage Devices.</p>
        </div>
    );
};

export default AlarmInstructions;
