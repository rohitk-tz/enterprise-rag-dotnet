import tempfile
from pathlib import Path

from fastapi import FastAPI, Form, HTTPException, UploadFile
from unstructured.chunking.title import chunk_by_title
from unstructured.partition.docx import partition_docx
from unstructured.partition.html import partition_html
from unstructured.partition.md import partition_md
from unstructured.partition.pdf import partition_pdf
from unstructured.partition.pptx import partition_pptx
from unstructured.partition.text import partition_text

app = FastAPI(title="Parsing Service")


def partition_document(temp_file: str, file_type: str, source_type: str = "file"):
    if source_type == "url":
        return partition_html(filename=temp_file)
    if file_type == "pdf":
        return partition_pdf(
            filename=temp_file,
            strategy="hi_res",
            infer_table_structure=True,
            extract_image_block_types=["Image"],
            extract_image_block_to_payload=True,
        )
    if file_type == "docx":
        return partition_docx(filename=temp_file, strategy="hi_res", infer_table_structure=True)
    if file_type == "pptx":
        return partition_pptx(filename=temp_file, strategy="hi_res", infer_table_structure=True)
    if file_type == "txt":
        return partition_text(filename=temp_file)
    if file_type == "md":
        return partition_md(filename=temp_file)
    raise HTTPException(status_code=400, detail=f"Unsupported file_type: {file_type}")


def analyze_elements(elements):
    counts = {"text": 0, "tables": 0, "images": 0, "titles": 0, "other": 0}
    for element in elements:
        name = type(element).__name__
        if name == "Table":
            counts["tables"] += 1
        elif name == "Image":
            counts["images"] += 1
        elif name in ("Title", "Header"):
            counts["titles"] += 1
        elif name in ("NarrativeText", "Text", "ListItem", "FigureCaption"):
            counts["text"] += 1
        else:
            counts["other"] += 1
    return counts


def separate_content_types(chunk, source_type="file"):
    is_url_source = source_type == "url"
    data = {"text": chunk.text, "tables": [], "images": [], "types": ["text"]}

    if hasattr(chunk, "metadata") and hasattr(chunk.metadata, "orig_elements"):
        for element in chunk.metadata.orig_elements:
            element_type = type(element).__name__
            if element_type == "Table":
                data["types"].append("table")
                table_html = getattr(element.metadata, "text_as_html", element.text)
                data["tables"].append(table_html)
            elif element_type == "Image" and not is_url_source:
                if (
                    hasattr(element, "metadata")
                    and hasattr(element.metadata, "image_base64")
                    and element.metadata.image_base64 is not None
                ):
                    data["types"].append("image")
                    data["images"].append(element.metadata.image_base64)

    data["types"] = list(set(data["types"]))
    return data


def get_page_number(chunk, chunk_index):
    if hasattr(chunk, "metadata"):
        page_number = getattr(chunk.metadata, "page_number", None)
        if page_number is not None:
            return page_number
    return chunk_index + 1


@app.post("/partition-and-chunk")
async def partition_and_chunk(
    file: UploadFile, file_type: str = Form(...), source_type: str = Form("file")
):
    suffix = ".html" if source_type == "url" else f".{file_type}"

    with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
        tmp.write(await file.read())
        temp_path = tmp.name

    try:
        elements = partition_document(temp_path, file_type, source_type)
        elements_summary = analyze_elements(elements)

        chunks = chunk_by_title(
            elements,
            max_characters=3000,
            new_after_n_chars=2400,
            combine_text_under_n_chars=500,
        )

        result_chunks = []
        for i, chunk in enumerate(chunks):
            content_data = separate_content_types(chunk, source_type)
            result_chunks.append(
                {
                    "text": content_data["text"],
                    "tables": content_data["tables"],
                    "images": content_data["images"],
                    "types": content_data["types"],
                    "page_number": get_page_number(chunk, i),
                    "char_count": len(content_data["text"]),
                }
            )

        return {
            "chunks": result_chunks,
            "chunking_metrics": {"total_chunks": len(chunks)},
            "elements_summary": elements_summary,
        }
    finally:
        Path(temp_path).unlink(missing_ok=True)
