// (Sau này bạn có thể thêm hiệu ứng hoặc nút chat Zalo ở đây)
console.log("Trang GHN đã tải xong!");
// Cuộn mượt đến section khi click menu
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
  anchor.addEventListener('click', function (e) {
    const targetId = this.getAttribute('href');
    if (targetId.length > 1) {
      e.preventDefault();
      const targetElement = document.querySelector(targetId);
      if (targetElement) {
        window.scrollTo({
          top: targetElement.offsetTop - 80,
          behavior: 'smooth'
        });
      }
    }
  });
});

// Hiệu ứng fade khi cuộn tới
const fadeSections = document.querySelectorAll('.fade-section');

function checkFadeSections() {
  const triggerBottom = window.innerHeight * 0.85;
  fadeSections.forEach(section => {
    const sectionTop = section.getBoundingClientRect().top;
    if (sectionTop < triggerBottom) {
      section.classList.add('show');
    }
  });
}

window.addEventListener('scroll', checkFadeSections);
window.addEventListener('load', checkFadeSections);
