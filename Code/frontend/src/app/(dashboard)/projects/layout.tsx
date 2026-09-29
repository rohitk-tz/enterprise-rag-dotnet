import { Sidebar } from "@/components/layout/Sidebar";
import { auth } from "@clerk/nextjs/server";
import { redirect } from "next/navigation";



export default async function DashboardLayout({ 
  children 
}: Readonly<{ children: React.ReactNode }>) {
  
    const {userId} = await auth();
    if(!userId){
       redirect("/sign-in");
    }
  return(
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <main className="flex-1 flex flex-col">{children}</main>
    </div>        
  );
}