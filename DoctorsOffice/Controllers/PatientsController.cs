using Microsoft.AspNetCore.Mvc;
using DoctorsOffice.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DoctorsOffice.Controllers
{
  public class PatientsController : Controller
  {
    private readonly DoctorsOfficeContext _db;

    public PatientsController(DoctorsOfficeContext db)
    {
      _db = db;
    }

    public ActionResult Index()
    {
      return View(_db.Patients.ToList());
    }

    public ActionResult Details(int id)
    {
      Patient thisPatient = _db.Patients
          .Include(patient => patient.JoinEntities)
          .ThenInclude(join => join.Doctor)
          .FirstOrDefault(patient => patient.PatientId == id);
      return View(thisPatient);
    }

    public ActionResult Create()
    {
      ViewBag.DoctorId = new SelectList(_db.Doctors, "DoctorId", "Name");
      return View();
    }

    [HttpPost] //altered to become a many to many relationship -- took logic from AddDoctor in order and saved Patient Id first, then proceed to add a doctor.
    public ActionResult Create(Patient patient, int doctorId)
    {
      _db.Patients.Add(patient);
      _db.SaveChanges();
      #nullable enable
      DoctorPatient? joinEntity = _db.DoctorPatients.FirstOrDefault(join => (join.DoctorId == doctorId && join.PatientId == patient.PatientId));
      #nullable disable
      if (joinEntity == null && doctorId != 0)
      {
        _db.DoctorPatients.Add(new DoctorPatient() { DoctorId = doctorId, PatientId = patient.PatientId });
        _db.SaveChanges();
      }
      return RedirectToAction("Index");
    }

    public ActionResult AddDoctor(int id)
    {
      Patient thisPatient = _db.Patients.FirstOrDefault(Patients => Patients.PatientId == id);
      ViewBag.DoctorId = new SelectList(_db.Doctors, "DoctorId", "Name");
      return View(thisPatient);
    }

    [HttpPost]
    public ActionResult AddDoctor(Patient patient, int doctorId)
    {
#nullable enable
      DoctorPatient? joinEntity = _db.DoctorPatients.FirstOrDefault(join => (join.DoctorId == doctorId && join.PatientId == patient.PatientId));
#nullable disable
      if (joinEntity == null && doctorId != 0)
      {
        _db.DoctorPatients.Add(new DoctorPatient() { DoctorId = doctorId, PatientId = patient.PatientId });
        _db.SaveChanges();
      }
      return RedirectToAction("Details", new { id = patient.PatientId });
    }

    public ActionResult Edit(int id)
    {
      Patient thisPatient = _db.Patients.FirstOrDefault(patients => patients.PatientId == id);
      return View(thisPatient);
    }

    [HttpPost]
    public ActionResult Edit(Patient patient)
    {
      _db.Patients.Update(patient);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
      Patient thisPatient = _db.Patients.FirstOrDefault(patients => patients.PatientId == id);
      return View(thisPatient);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      Patient thisPatient = _db.Patients.FirstOrDefault(patients => patients.PatientId == id);
      _db.Patients.Remove(thisPatient);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult DeleteJoin(int joinId)
    {
      DoctorPatient joinEntry = _db.DoctorPatients.FirstOrDefault(entry => entry.DoctorPatientId == joinId);
      _db.DoctorPatients.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}