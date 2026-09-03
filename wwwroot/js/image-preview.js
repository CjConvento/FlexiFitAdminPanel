/**
 * Preview image before upload
 * @param {HTMLInputElement} input - The file input element
 */
function previewImage(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function(e) {
            // Find the preview image element
            var preview = document.getElementById('imagePreview');
            if (preview) {
                preview.src = e.target.result;
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Optional: Auto-initialize all file inputs with data-preview attribute
document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('input[type="file"][data-preview]').forEach(function(input) {
        input.addEventListener('change', function() {
            previewImage(this);
        });
    });
});