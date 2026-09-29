"use client"
import { useCallback, useEffect, useState } from 'react'
import { useAuth } from '@clerk/nextjs'
import { useRouter } from 'next/navigation'

import { ProjectsGrid } from '@/components/projects/ProjectsGrid'
import { CreateProjectModal } from '@/components/projects/CreateProjectModal'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { Project } from '@/lib/types'

import toast from 'react-hot-toast'
import { apiClient } from '../../../lib/api/index';



const ProjectsPage = () => {
 //data state
 const [projects, setProjects] = useState<Project[]>([])
 const [loading, setLoading] = useState<boolean>(true)
 const [error , setError] = useState<string | null>(null)

 //ui state
 const [searchQuery, setSearchQuery] = useState<string>("")
 const [viewMode, setViewMode] = useState<"grid" | "list">("grid")

 // Modal state
 const [showCreateModal, setShowCreateModal] = useState<boolean>(false)
 const [isCreating, setIsCreating] = useState<boolean>(false)
 
 const { getToken, userId } = useAuth()
 const router = useRouter();

 // backend logic 
 const loadProjects =async () => { 
  setLoading(true);
  setError(null);
  try {
    const token = await getToken();

    console.log("Auth Token:", token);

    const response = await apiClient.get(`/api/projects/`, token);
    
    const {data} = response || {};

    setProjects(data);

  } catch (error) {
    console.error("Failed to load projects", error);
    toast.error("Failed to load projects. Please try again.");
    setError("Failed to load projects. Please try again.");
  } finally {
    setLoading(false);
  }
 }

 const createProjects = async (name: string, description: string) => {
    if(!userId) return;
    setIsCreating(true);
    try {
      const token = await getToken();
      const response = await apiClient.post(`/api/projects/`, token, { name, description });   
     console.log("Create Project Response:", response);
      if(!response){
        throw new Error("Failed to create project.")
      }
      await loadProjects();
      setShowCreateModal(false);
      toast.success("Project created successfully.");
    } catch (error) {
      console.error("Failed to create project", error);
      toast.error("Failed to create project. Please try again.");
    } finally {
      setIsCreating(false);
    }
  }

  const handleDeleteProject = async (projectId: string) => {
    if(!userId) return;
    try{
      const token = await getToken();
      const response = await apiClient.delete(`/api/projects/${projectId}`, token);
      if(!response){
        throw new Error("Failed to delete project.");
      }
      setProjects((prev) => prev.filter((project)=> project.id !== projectId));
      toast.success("Project deleted successfully.");
    }catch(error){
      console.error("Failed to delete project", error);
      toast.error("Failed to delete project. Please try again.");
    }
  }
  const handleProjectClick = (projectId: string) => {
    router.push(`/projects/${projectId}`);
  }

  const handleCreateOpenModal = () => {
    setShowCreateModal(true);
  }
  const handleCreateCloseModal = () => {
    setShowCreateModal(false);
  }

  useEffect(() => {
    if(userId)
    loadProjects();
  }, [userId]);

  const filteredProjects = projects.filter((project) =>
    project.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if(loading && filteredProjects.length === 0){
    return (     
        <LoadingSpinner message='Loading projects...' />
    )
  }

 return (
    <div>
     <ProjectsGrid 
      projects={filteredProjects}
      loading={loading}
      error={error}
      searchQuery={searchQuery}      
      viewMode={viewMode}
      onSearchChange={setSearchQuery}
      onViewModeChange={setViewMode}
      onProjectClick={handleProjectClick}
      onDeleteProject={handleDeleteProject}
      onCreateProject={handleCreateOpenModal}      
     />
     <CreateProjectModal 
        isOpen={showCreateModal}
        onClose={handleCreateCloseModal}
        onCreateProject={createProjects}
        isLoading={isCreating}
      />
     {loading && (
      <div className="flex justify-center mt-4">
        <LoadingSpinner />
      </div>
     )}
     
    
    </div>
  )

}
export default ProjectsPage