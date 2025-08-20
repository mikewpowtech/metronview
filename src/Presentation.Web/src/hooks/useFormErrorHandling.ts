import { useState } from 'react';
import { message } from 'antd';

interface ValidationError {
    field: string;
    message: string;
}

interface ApiError {
    message: string;
    errors?: Record<string, string[]>;
    validationErrors?: ValidationError[];
}

export const useFormErrorHandling = () => {
    const [validationErrors, setValidationErrors] = useState<Record<string, string[]>>({});
    const [formErrors, setFormErrors] = useState<string[]>([]);

    const parseApiError = (error: any): ApiError => {
        if (error.response?.data) {
            const { data } = error.response;
            
            if (data.errors) {
                return {
                    message: data.message || 'Validation failed',
                    errors: data.errors
                };
            }
            
            if (data.validationErrors) {
                const errors: Record<string, string[]> = {};
                data.validationErrors.forEach((validationError: ValidationError) => {
                    if (!errors[validationError.field]) {
                        errors[validationError.field] = [];
                    }
                    errors[validationError.field].push(validationError.message);
                });
                
                return {
                    message: data.message || 'Validation failed',
                    errors
                };
            }
            
            return {
                message: data.message || error.message || 'An error occurred'
            };
        }
        
        return {
            message: error.message || 'An unknown error occurred'
        };
    };

    const handleApiError = (error: any): boolean => {
        const apiError = parseApiError(error);
        
        if (apiError.errors) {
            setValidationErrors(apiError.errors);
            message.error(apiError.message);
            return false; // Don't close modal
        } else {
            message.error(apiError.message);
            return !apiError.message.toLowerCase().includes('validation'); // Close modal for non-validation errors
        }
    };

    const handleFormError = (error: any): boolean => {
        if (error.errorFields) {
            const fieldErrors = error.errorFields.map((field: any) => field.errors[0]);
            setFormErrors(fieldErrors);
            return false; // Don't close modal
        }
        return true;
    };

    const clearErrors = () => {
        setValidationErrors({});
        setFormErrors([]);
    };

    return {
        validationErrors,
        formErrors,
        handleApiError,
        handleFormError,
        clearErrors,
        setFormErrors
    };
};