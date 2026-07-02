function printChartElement(element) {
    if (element) {
        const printWindow = window.open('', '_blank');
        printWindow.document.write('<html><head><title>Print Chart</title></head><body>');
        printWindow.document.write(element.outerHTML);
        printWindow.document.write('</body></html>');
        printWindow.document.close();
        printWindow.print();
    } else {
        console.error('Chart element not found.');
    }
}

 
