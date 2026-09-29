import { auth } from "@clerk/nextjs/server";
import { redirect } from "next/navigation";

// Ensure this page renders on each request so Clerk can read cookies
export const dynamic = "force-dynamic";

async function HomePage() {
  const { userId } = await auth();
  console.log("User ID:", userId);
  // http://localhost:3000/sign-in/sso
  // http://localhost:3000/sign-in/password-reset

  if (userId) {
    redirect("/projects");
  } else {
    redirect("/sign-in");
  }
}

export default HomePage;
