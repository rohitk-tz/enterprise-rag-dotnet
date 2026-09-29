// Basic API client function with authentication support
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";

export const apiClient = {
    get : async (endpoint: string, token?: string|null) => {
        const headers: HeadersInit = {};
        if(token){
             console.log("token in apiClient.get:", token);
            headers['Authorization'] = `Bearer ${token}`;
        }
       console.log("Fetching from:", `${API_BASE_URL}${endpoint}`, "with headers:", headers);
        const response = await fetch(`${API_BASE_URL}${endpoint}`, { headers });
        if(!response.ok){
            throw new Error(`API GET request failed: ${response.statusText}`);
        }
        return response.json();
    },
    post : async (endpoint: string, token?: string|null, body?: any) => {
        const headers: HeadersInit = {
            "Content-Type": "application/json"
        };        
        if(token){
            headers['Authorization'] = `Bearer ${token}`;
        }
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: "POST",
            body: JSON.stringify(body),
            headers
        }); 
        if(!response.ok){
            throw new Error(`API POST request failed: ${response.statusText}`);
        }
        return response.json();
    },
    delete : async (endpoint: string, token?: string|null) => {
        const headers: HeadersInit = {
            "Content-Type": "application/json"
        };        
        if(token){
            headers['Authorization'] = `Bearer ${token}`;
        }
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: "DELETE",
            headers
        }); 
        if(!response.ok){
            throw new Error(`API DELETE request failed: ${response.statusText}`);
        }
        return response.json();
    },
    put : async (endpoint: string, token?: string|null, body?: any) => {
        const headers: HeadersInit = {
            "Content-Type": "application/json"
        };        
        if(token){
            headers['Authorization'] = `Bearer ${token}`;
        }
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: "PUT",
            body: JSON.stringify(body),
            headers
        }); 
        if(!response.ok){
            throw new Error(`API PUT request failed: ${response.statusText}`);
        }
        return response.json();
    },
    uploadToS3 : async (uploadUrl: string, file: File) => {
        const response = await fetch(uploadUrl, {
            method: "PUT",
            body: file,
            headers: {
                "Content-Type": file.type
            }
        });
        if(!response.ok){
            throw new Error(`S3 upload failed: ${response.statusText}`);
        }
        return response;
    }
}