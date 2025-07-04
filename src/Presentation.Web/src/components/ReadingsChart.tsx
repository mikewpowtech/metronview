import React from "react";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import type { Reading } from "../features/readings/readingAPI";
import dayjs from "dayjs";

interface ReadingsChartProps {
    readings: Reading[];
}

const ReadingsChart: React.FC<ReadingsChartProps> = ({ readings }) => {
    // Process readings data for chart
    const chartData = readings
        .map(reading => ({
            dateRecorded: reading.dateRecordedUtc,
            value: reading.value,
            sensorName: reading.sensor?.name || `Sensor ${reading.sensor?.id}`,
            sensorId: reading.sensor?.id,
        }))
        .sort((a, b) => new Date(a.dateRecorded).getTime() - new Date(b.dateRecorded).getTime());

    // Get unique sensors for different colored lines
    const uniqueSensors = Array.from(
        new Set(chartData.map(item => item.sensorId))
    ).map(id => {
        const sensor = chartData.find(item => item.sensorId === id);
        return {
            id,
            name: sensor?.sensorName || `Sensor ${id}`,
        };
    });

    // Create data points grouped by date
    const groupedData: { [key: string]: any } = {};
    
    chartData.forEach(item => {
        const dateKey = dayjs(item.dateRecorded).format("YYYY-MM-DD HH:mm");
        if (!groupedData[dateKey]) {
            groupedData[dateKey] = { date: dateKey };
        }
        groupedData[dateKey][`sensor_${item.sensorId}`] = item.value;
    });

    const finalChartData = Object.values(groupedData);

    // Color palette for different sensors
    const colors = [
        "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
        "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf"
    ];

    const formatTooltipLabel = (label: string) => {
        return `Date: ${label}`;
    };

    const formatTooltip = (value: any, name: string) => {
        const sensorId = name.replace("sensor_", "");
        const sensor = uniqueSensors.find(s => s.id?.toString() === sensorId);
        return [value, sensor?.name || `Sensor ${sensorId}`];
    };

    return (
        <div style={{ width: "100%", height: "400px" }}>
            <ResponsiveContainer>
                <LineChart data={finalChartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                        dataKey="date" 
                        angle={-45}
                        textAnchor="end"
                        height={80}
                        interval="preserveStartEnd"
                    />
                    <YAxis />
                    <Tooltip 
                        labelFormatter={formatTooltipLabel}
                        formatter={formatTooltip}
                    />
                    <Legend />
                    {uniqueSensors.map((sensor, index) => (
                        <Line
                            key={sensor.id}
                            type="monotone"
                            dataKey={`sensor_${sensor.id}`}
                            stroke={colors[index % colors.length]}
                            strokeWidth={2}
                            name={sensor.name}
                            connectNulls={false}
                            dot={{ r: 3 }}
                        />
                    ))}
                </LineChart>
            </ResponsiveContainer>
        </div>
    );
};

export default ReadingsChart;
