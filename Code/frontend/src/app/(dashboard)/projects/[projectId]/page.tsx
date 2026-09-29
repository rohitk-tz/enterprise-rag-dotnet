"use client";

import React, { use, useEffect, useState } from "react";
import { ConversationsList } from "@/components/projects/ConversationsList";
import { KnowledgeBaseSidebar } from "@/components/projects/KnowledgeBaseSidebar";
import { FileDetailsModal } from "@/components/projects/FileDetailsModal";
import { useAuth } from "@clerk/nextjs";
import { apiClient } from "@/lib/api";
import { LoadingSpinner } from "@/components/ui/LoadingSpinner";
import { NotFound } from "@/components/ui/NotFound";
import toast from "react-hot-toast";
import { Chat, Project, ProjectDocument, ProjectSettings } from "@/lib/types";
import {useRouter} from "next/navigation";

interface ProjectPageProps {
  params: Promise<{
    projectId: string;
  }>;
}
interface ProjectData {
  project: Project | null;
  chats: Chat[];
  documents: ProjectDocument[];
  settings: ProjectSettings | null;
}

function ProjectPage({ params }: ProjectPageProps) {
  const { projectId } = use(params);

  const { getToken, userId } = useAuth();
  const router = useRouter();

  // Data state
  const [data, setData] = useState<ProjectData>({
    project: null,
    chats: [],
    documents: [],
    settings: null,
  });

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreatingChat, setIsCreatingChat] = useState(false);
  // UI states
  const [activeTab, setActiveTab] = useState<"documents" | "settings">(
    "documents",
  );

  const [selectedDocumentId, setSelectedDocumentId] = useState<string | null>(
    null,
  );

  React.useEffect(() => {
    const loadAllData = async () => {
      if (!userId) return;

      try {
        setLoading(true);
        setError(null);
        const token = await getToken();
        await Promise.all([
          // Load Project Details
          (async () => {
            const res = await apiClient.get(
              `/api/projects/${projectId}`,
              token,
            );
            setData((prev) => ({ ...prev, project: res.data }));
          })(),
          // Load Chats
          (async () => {
            const res = await apiClient.get(
              `/api/projects/${projectId}/chats`,
              token,
            );
            setData((prev) => ({ ...prev, chats: res.data }));
          })(),
          // Load Documents
          (async () => {
            const res = await apiClient.get(
              `/api/projects/${projectId}/files`,
              token,
            );
            setData((prev) => ({ ...prev, documents: res.data }));
          })(),
          // Load Settings
          (async () => {
            const res = await apiClient.get(
              `/api/projects/${projectId}/settings`,
              token,
            );
            setData((prev) => ({ ...prev, settings: res.data }));
          })(),
        ]);
      } catch (error) {
        console.error("Failed to load project data", error);
        setError("Failed to load project data. Please try again.");
      } finally {
        setLoading(false);
      }
    };
    loadAllData();
  }, [projectId, userId]);

  /*
   * Short Polling
   */
  useEffect(() => {
    const hasProcessingDocuments = data.documents.some(
      (doc) =>
        doc.processing_status &&
        !["completed", "failed"].includes(doc.processing_status)
    );

    if (!hasProcessingDocuments) {
      return;
    }

    const pollInterval = setInterval(async () => {
      try {
        const token = await getToken();
        const documentsRes = await apiClient.get(
          `/api/projects/${projectId}/files`,
          token
        );

        setData((prev) => ({
          ...prev,
          documents: documentsRes.data,
        }));
      } catch (err) {
        console.error("Polling error:", err);
      }
    }, 2000);

    return () => clearInterval(pollInterval);
  }, [data.documents, projectId, getToken]);

  //   Chat-related methods
  const handleCreateNewChat = async () => {
    if (!userId) return;

    try {
      setIsCreatingChat(true);
      const token = await getToken();
      const chatNumber = Date.now() % 10000;

      const res = await apiClient.post(`/api/chats`, token, {
        title: `Chat #${chatNumber}`,
        project_id: projectId,
      });

      setData((prev) => ({
        ...prev,
        chats: [res.data, ...prev.chats],
      }));
      toast.success("New chat created successfully");
    } catch (error) {
      console.error("Failed to create new chat", error);
    } finally {
      setIsCreatingChat(false);
    }
  };

  const handleDeleteChat = async (chatId: string) => {
    if (!userId) return;

    try {
      const token = await getToken();

      await apiClient.delete(`/api/chats/${chatId}`, token);

      // Update local state
      setData((prev) => ({
        ...prev,
        chats: prev.chats.filter((chat) => chat.id !== chatId),
      }));

      toast.success("Chat deleted successfully");
    } catch (err) {
      toast.error("Failed to delete chat");
    }
  };

  const handleChatClick = (chatId: string) => {   
    router.push(`/projects/${projectId}/chats/${chatId}`);
  };

  //   Document-related methods
  const handleDocumentUpload = async (files: File[]) => {
    if (!userId) return;
    const token = await getToken();
    const uploadedDocuments: ProjectDocument[] = [];
    // process each file parallelly
    try {
      const uploadPromises = files.map(async (file) => {
        try {
          const res = await apiClient.post(
            `/api/projects/${projectId}/files/upload-url`,
            token,
            {
              filename: file.name,
              file_type: file.type,
              file_size: file.size,
            },
          );
          await apiClient.uploadToS3(res.upload_url, file);
          const uploadRes = await apiClient.post(
            `/api/projects/${projectId}/files/confirm-upload`,
            token,
            {
              s3_key: res.s3_key,
            },
          );
          uploadedDocuments.push(uploadRes.data);
        } catch (error) {
          console.error(`Failed to upload file ${file.name}`, error);
        }
      });

      await Promise.allSettled(uploadPromises);

      console.log("All files processed", uploadedDocuments.length);
      // Update local state
      if (uploadedDocuments.length > 0) {
        setData((prev) => ({
          ...prev,
          documents: [...uploadedDocuments, ...prev.documents],
        }));
      }

      toast.success("Documents uploaded successfully");
    } catch (error) {
      console.error("Failed to upload documents", error);
      toast.error("Failed to upload documents");
    }
  };

  const handleDocumentDelete = async (documentId: string) => {
    if (!userId) return;
    try {
      const token = await getToken();
      await apiClient.delete(
        `/api/projects/${projectId}/files/${documentId}`,
        token,
      );
      setData((prev) => ({
        ...prev,
        documents: prev.documents.filter((doc) => doc.id !== documentId),
      }));
      toast.success("Document deleted successfully");
    } catch (error) {
      console.error("Failed to delete document", error);
      toast.error("Failed to delete document");
    }
  };

  const handleUrlAdd = async (url: string) => {
   if (!userId) return;

    try {
      const token = await getToken();
      const res = await apiClient.post(
        `/api/projects/${projectId}/urls`,
        token,  
        { url } );
      setData((prev) => ({
        ...prev,
        documents: [res.data, ...prev.documents],
      }));
      toast.success("URL added successfully");
    }catch (error) {
      console.error("Failed to add URL", error);
      toast.error("Failed to add URL");
    }
      
  };

  const handleOpenDocument = (documentId: string) => {
    console.log("Open document", documentId);
    setSelectedDocumentId(documentId);
  };

  // Project settings

  const handleDraftSettings = (updates: any) => {
    setData((prev) => ({
      ...prev,
      settings: { ...prev.settings, ...updates } as ProjectSettings,
    }));
  };

  const handlePublishSettings = async () => {
    if (userId == null || data.settings == null) return;

    try {
      const token = await getToken();
      const result = await apiClient.put(
        `/api/projects/${projectId}/settings`,
        token,
        data.settings,
      );
      setData((prev) => ({
        ...prev,
        settings: result.data,
      }));
      toast.success("Project settings updated successfully");
    } catch (error) {
      console.error("Failed to update project settings", error);
      toast.error("Failed to update project settings");
    } finally {
      // No-op
    }
  };

  if (loading) {
    return <LoadingSpinner message="Loading project data..." />;
  }
  if (!data.project) {
    return <NotFound message="Project not found." />;
  }

  const selectedDocument = selectedDocumentId
    ? data.documents.find((doc) => doc.id == selectedDocumentId)
    : null;

  return (
    <>
      <div className="flex h-screen bg-[#0d1117] gap-4 p-4">
        <ConversationsList
          project={data.project}
          conversations={data.chats}
          error={error}
          loading={isCreatingChat}
          onCreateNewChat={handleCreateNewChat}
          onChatClick={handleChatClick}
          onDeleteChat={handleDeleteChat}
        />

        {/* KnowledgeBase Sidebar */}
        <KnowledgeBaseSidebar
          activeTab={activeTab}
          onSetActiveTab={setActiveTab}
          projectDocuments={data.documents}
          onDocumentUpload={handleDocumentUpload}
          onDocumentDelete={handleDocumentDelete}
          onOpenDocument={handleOpenDocument}
          onUrlAdd={handleUrlAdd}
          projectSettings={data.settings}
          settingsError={error}
          settingsLoading={false}
          onUpdateSettings={handleDraftSettings}
          onApplySettings={handlePublishSettings}
        />
      </div>
      {selectedDocument && (
        <FileDetailsModal
          document={selectedDocument}
          onClose={() => setSelectedDocumentId(null)}
        />
      )}
    </>
  );
}

export default ProjectPage;
