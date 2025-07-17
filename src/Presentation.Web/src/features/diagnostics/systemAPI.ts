import axios from "axios";

const BASE_URL = import.meta.env.REACT_APP_API_URL as string;

/**
 * Posts data to the EchoController and returns the echoed response.
 * @param data The data to echo (any JSON-serializable object)
 */
export const echoApi = async (data: any) => {
    try {
        const response = await axios.post(`${BASE_URL}/echo/echo`, data);
        return response.data;
    } catch (ex) {
        console.log(ex);
        return null;
    }
};

/**
 * Gets an echoed message from the EchoController using a GET request.
 * @param message The message to echo (as a query parameter)
 */
export const echoApiGet = async (message: string) => {
    const response = await axios.get(`${BASE_URL}/echo/echo`, {
        params: { message }
    });
    return response.data;
};

/**
 * Fetches a random cat fact from catfact.ninja.
 */
export const getCatFact = async () => {
    const response = await axios.get("https://catfact.ninja/fact");
    return response.data;
};