import React from 'react';
import { Form } from 'antd';
import { Company } from '../../../features/companies/companyAPI';
import { Unit } from '../../../features/units/unitsAPI';
import { Alarm } from '../../../features/alarms/alarmAPI';
import { UnitModel } from '../../../features/unitmodels/unitModelAPI';
import { CompanyModal } from '../../../features/companies/CompanyModal';
import { UnitListModal } from '../../../features/units/UnitListModal';
import { AlarmModal } from '../../../features/alarms/AlarmModal';
import { TriggerModal } from '../../../features/triggers/TriggerModal';
import { NodeType, VisualizationNode } from '../../../types/visualization';

interface VisualizationModalsProps {
    editingNode: VisualizationNode | null;
    editModalOpen: boolean;
    modalLoading: boolean;
    companyForm: any;
    unitForm: any;
    alarmForm: any;
    triggerForm: any;
    companies: Company[];
    unitModels: UnitModel[];
    alarms: Alarm[];
    onOk: () => Promise<void>;
    onCancel: () => void;
}

export const VisualizationModals: React.FC<VisualizationModalsProps> = ({
    editingNode,
    editModalOpen,
    modalLoading,
    companyForm,
    unitForm,
    alarmForm,
    triggerForm,
    companies,
    unitModels,
    alarms,
    onOk,
    onCancel
}) => {
    return (
        <>
            {editingNode?.type === NodeType.COMPANY && (
                <CompanyModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={companyForm}
                    companies={companies}
                    onOk={onOk}
                    onCancel={onCancel}
                />
            )}

            {editingNode?.type === NodeType.UNIT && (
                <UnitListModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={unitForm}
                    companies={companies}
                    unitModels={unitModels}
                    onOk={onOk}
                    onCancel={onCancel}
                />
            )}

            {editingNode?.type === NodeType.ALARM && (
                <AlarmModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={alarmForm}
                    companies={companies}
                    onOk={onOk}
                    onCancel={onCancel}
                />
            )}

            {editingNode?.type === NodeType.TRIGGER && (
                <TriggerModal
                    showModal={editModalOpen}
                    isEdit={true}
                    modalLoading={modalLoading}
                    form={triggerForm}
                    alarms={alarms}
                    triggerTypes={[]}
                    communicationModes={[]}
                    onOk={onOk}
                    onCancel={onCancel}
                />
            )}
        </>
    );
};